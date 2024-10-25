using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientServer
{

    public class Server<TUserCommand> : BaseClientServer<TUserCommand>
    {
        Thread workThread;

        public Action<string> NewUserConnect;

        public Server(string ip, int port = Utils.defaultPort)
            : base(ip, port)
        {
            Token.Value = TokenGenerator.Generate();
        }

        public override void Start()
        {
            base.Start();
            workThread = new Thread(Work);
            workThread.Start();
        }

        public override void Stop()
        {
            base.Stop();
            mainSocket.Close();
            workThread.Abort();
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
                    if (TryGetMessageForToken(token, out Message<TUserCommand> message))
                    {
                        Utils.SendPackage(handler, message.GetJson());

                        if (message.MessageType != Message<TUserCommand>.GeneralMessageType.Ping
                            && message.MessageType != Message<TUserCommand>.GeneralMessageType.FileProgress)
                        {
                            Log("send: " + message.GetJson());
                        }
                    }
                    else
                    {
                        Thread.Sleep(5);
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
                    if (messageFrom.MessageType != Message<TUserCommand>.GeneralMessageType.Ping
                        && messageFrom.MessageType != Message<TUserCommand>.GeneralMessageType.FileProgress)
                    {
                        Log("recv: " + data);
                    }

                    CheckRecvMessage(messageFrom);

                    if (messageFrom.MessageType == Message<TUserCommand>.GeneralMessageType.GetReg)
                    {
                        token = TokenGenerator.Generate();

                        var ans = new Message<TUserCommand>(Token.Value, token, Message<TUserCommand>.GeneralMessageType.SendReg)
                            .Add("token", token);


                        NewUserConnect?.Invoke(token);
                        SendMessage(ans);

                        new Task(() => SendThread(handler, token)).Start();
                    }
                }
            }
            catch (Exception ex)
            {
                OnError(token, handler, ex);
            }
        }

        private void OnError(string token, Socket socket, Exception ex)
        {
            socket.Shutdown(SocketShutdown.Both);
            Log("error " + ex.ToString() + " \n" + ex.StackTrace);
            onErrorAction(ex, token);
        }

        private void Work()
        {
            mainSocket.Bind(ipEndPoint);
            mainSocket.Listen(100);

            while (true)
            {
                Socket handler = mainSocket.Accept();
                new Task(() => RecvThread(handler)).Start();
            }
        }

        private void Log(string data)
        {
            Console.WriteLine(data);
        }

    }
}
