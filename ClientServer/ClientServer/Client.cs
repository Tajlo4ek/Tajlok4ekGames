using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ClientServer
{
    public class Client<TUserCommand> : BaseClientServer<TUserCommand>
    {
        private readonly Tajlo4ekUtils.AtomicValue<string> serverToken = new Tajlo4ekUtils.AtomicValue<string>("");

        public string ServerToken { get { return serverToken.Value; } }


        public Action<Message<TUserCommand>> OnGetMessage;
        public Action OnServerConnected;

        Thread workSendThread;
        Thread workRecvThread;

        public Client(string ip, int port = Utils.defaultPort)
            : base(ip, port)
        {
        }

        public override void Start()
        {
            base.Start();

            mainSocket.Connect(ipEndPoint);

            workSendThread = new Thread(SendThread);
            workRecvThread = new Thread(RecvThread);

            workSendThread.Start();
            workRecvThread.Start();
        }


        private void SendThread()
        {
            var getRegTimeout = DateTime.Now.AddSeconds(30);

            bool isSendStart = false;

            try
            {
                while (NeedStop == false)
                {
                    if (Token.Value.Length == 0)
                    {
                        if (isSendStart == false)
                        {
                            isSendStart = true;

                            var startMessage = new Message<TUserCommand>("", "", Message<TUserCommand>.GeneralMessageType.GetReg);

                            Utils.SendPackage(mainSocket, startMessage.GetJson());
                        }

                        if (getRegTimeout < DateTime.Now)
                        {
                            throw new Exception("timeout reg token");
                        }
                        else
                        {
                            Thread.Sleep(50);
                        }
                    }
                    else
                    {
                        if (TryGetMessageForToken(serverToken.Value, out Message<TUserCommand> message))
                        {
                            Utils.SendPackage(mainSocket, message.GetJson());

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
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void RecvThread()
        {
            try
            {
                while (NeedStop == false)
                {
                    var bytes = Utils.GetPackage(mainSocket);
                    var data = Encoding.UTF32.GetString(bytes);

                    var messageFrom = Message<TUserCommand>.FromJson(data);

                    if (messageFrom.MessageType != Message<TUserCommand>.GeneralMessageType.Ping
                        && messageFrom.MessageType != Message<TUserCommand>.GeneralMessageType.FileProgress)
                    {
                        Log("recv: " + data);
                    }

                    CheckRecvMessage(messageFrom);
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void OnError(Exception ex)
        {
            Log("error: " + ex.ToString() + "\n" + ex.StackTrace);

            Stop();

            onErrorAction(ex, Token.Value);

            workRecvThread.Abort();
            workSendThread.Abort();

            mainSocket.Shutdown(SocketShutdown.Both);
            mainSocket.Close();
        }

        protected override void CheckRecvMessage(Message<TUserCommand> message)
        {
            base.CheckRecvMessage(message);

            switch (message.MessageType)
            {
                case Message<TUserCommand>.GeneralMessageType.SendReg:
                    serverToken.Value = message.TokenFrom;
                    Token.Value = message.GetData<string>("token");
                    OnServerConnected?.Invoke();
                    break;

                case Message<TUserCommand>.GeneralMessageType.FileProgress:
                    CheckFileMessage(message);
                    break;

                case Message<TUserCommand>.GeneralMessageType.User:
                    OnGetMessage(message);
                    break;

                default:
                    break;
            }

        }

        private void Log(string data)
        {
            Console.WriteLine(data);
        }

    }
}


