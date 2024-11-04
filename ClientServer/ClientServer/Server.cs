using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientServer
{

    public class Server<TUserCommand> : BaseClientServer<TUserCommand>
    {
        public Action<string> NewUserConnect;

        private readonly ConcurrentDictionary<string, AutoResetEvent> sendEvents;

        public Server(IPAddress ip, int port = Utils.defaultPort)
            : base(ip, port)
        {
            Token.Value = TokenGenerator.Generate();
            sendEvents = new ConcurrentDictionary<string, AutoResetEvent>();
        }

        public override bool Start()
        {
            try
            {
                base.Start();

                mainSocket.Bind(ipEndPoint);
                mainSocket.Listen(100);
                AcceptCallback(null);
            }
            catch (Exception ex)
            {
                OnError("", null, ex);
                return false;
            }
            return true;
        }

        public override void Stop()
        {
            base.Stop();
            mainSocket?.Close();
        }

        protected override void CheckRecvMessage(Message<TUserCommand> message)
        {
            base.CheckRecvMessage(message);

            switch (message.MessageType)
            {
                case Message<TUserCommand>.GeneralMessageType.FileProgress:
                    CheckFileMessage(message);
                    break;

                case Message<TUserCommand>.GeneralMessageType.User:
                    onGetMessage(message);
                    break;

                default:
                    break;
            }
        }

        private void SendThread(Socket handler, string token)
        {
            try
            {
                while (NeedStop == false)
                {
                    sendEvents[token].WaitOne(1000);

                    if (TryGetMessageForToken(token, out Message<TUserCommand> message))
                    {
                        Utils.SendPackage(handler, message.GetJson());

                        if (message.MessageType != Message<TUserCommand>.GeneralMessageType.Ping
                            && message.MessageType != Message<TUserCommand>.GeneralMessageType.FileProgress)
                        {
                            Log("send: " + message.GetJson());
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                OnError(token, handler, ex);
            }
        }

        private void RecvThread(Socket handler)
        {
            string token = "";

            try
            {
                while (NeedStop == false)
                {
                    var bytes = Utils.GetPackage(handler);
                    var data = Encoding.UTF32.GetString(bytes);

                    var messageFrom = Message<TUserCommand>.FromJson(data);

                    if (messageFrom.MessageType == Message<TUserCommand>.GeneralMessageType.GetReg)
                    {
                        token = TokenGenerator.Generate();

                        var ans = new Message<TUserCommand>(Token.Value, token, Message<TUserCommand>.GeneralMessageType.SendReg)
                            .Add("token", token);

                        sendEvents[token] = RegNewToken(token, handler);

                        SendMessage(ans);
                        NewUserConnect?.Invoke(token);

                        new Task(() => SendThread(handler, token)).Start();
                    }

                    CheckRecvMessage(messageFrom);
                }
            }
            catch (Exception ex)
            {
                OnError(token, handler, ex);
            }
        }

        private void OnError(string token, Socket socket, Exception ex)
        {
            socket?.Shutdown(SocketShutdown.Both);
            Log("error " + ex.ToString() + " \n" + ex.StackTrace);
            onErrorAction?.Invoke(ex, token);
        }

        private void AcceptCallback(IAsyncResult result)
        {
            if (result != null)
            {
                Socket handler = mainSocket.EndAccept(result);
                new Task(() => RecvThread(handler)).Start();
            }

            mainSocket.BeginAccept(AcceptCallback, null);
        }
    }
}
