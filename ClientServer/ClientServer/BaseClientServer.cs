using ClientServer.FileUtils;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientServer
{
    public abstract class BaseClientServer<TUserCommand>
    {
        protected readonly Tajlo4ekUtils.AtomicValue<string> Token = new Tajlo4ekUtils.AtomicValue<string>("");
        protected readonly Tajlo4ekUtils.AtomicValue<bool> _needStop = new Tajlo4ekUtils.AtomicValue<bool>(false);

        protected bool NeedStop { get { return _needStop.Value; } }

        public string MyToken { get { return Token.Value; } }

        protected readonly IPEndPoint ipEndPoint;
        protected readonly Socket mainSocket;

        public Action<Exception, string> OnErrorAction;
        private readonly SendRecvController sendRecvController;

        public Action<Message<TUserCommand>> OnGetMessage;

        protected Action<string, Socket, AutoResetEvent> InternalNewConnectAction;
        public Action<string> NewConnectAction;

        public Action<ProgressFileData> OnFileLoadProgress
        {
            get { return sendRecvController.OnLoadCallback; }
            set { sendRecvController.OnLoadCallback = value; }
        }

        private string workPath;
        private readonly bool isServer;

        private readonly ConcurrentDictionary<string, ConcurrentQueueWithSignal<Message<TUserCommand>>> sendMessageQueue;
        private readonly ConcurrentQueueWithSignal<Message<TUserCommand>> recvMessageQueue;

        private class Connection
        {
            public Connection(string tokenMy, string tokenRemote)
            {
                UpdateCloseTime();
                UpdatePingTime();

                PingMessage = new Message<TUserCommand>(
                    tokenMy,
                    tokenRemote,
                    Message<TUserCommand>.GeneralMessageType.Ping);
            }

            public DateTime NextSendPingTime { get; private set; }
            public DateTime CloseConnectionTime { get; private set; }
            public Message<TUserCommand> PingMessage { get; private set; }

            public void UpdateCloseTime()
            {
                this.CloseConnectionTime = DateTime.Now.AddSeconds(15);
            }

            public void UpdatePingTime()
            {
                this.NextSendPingTime = DateTime.Now.AddSeconds(1);
            }
        }

        private readonly ConcurrentDictionary<string, Connection> connections;

        public BaseClientServer(IPAddress ipAddr, int port, bool isServer)
        {
            ipEndPoint = new IPEndPoint(ipAddr, port);
            mainSocket = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            sendMessageQueue = new ConcurrentDictionary<string, ConcurrentQueueWithSignal<Message<TUserCommand>>>();
            sendRecvController = new SendRecvController();

            connections = new ConcurrentDictionary<string, Connection>();
            recvMessageQueue = new ConcurrentQueueWithSignal<Message<TUserCommand>>();
            this.isServer = isServer;

            new Task(CheckRecvMessageThread).Start();
        }

        public void SendFile(string tokenTo, string fileName)
        {
            var message = new Message<TUserCommand>(MyToken, tokenTo, Message<TUserCommand>.GeneralMessageType.FileProgress);

            var path = workPath + @"\" + fileName;

            if (File.Exists(path))
            {
                var length = Utils.GetFileSize(path);

                var fileToken = sendRecvController.AddSendFile(tokenTo, path);

                message.Add("totalSize", length.ToString())
                       .Add("fileName", fileName)
                       .Add("fileToken", fileToken)
                       .Add("type", Message<TUserCommand>.FileProgressMessageType.SendFile);


                SendMessage(message);
            }

        }

        private void CheckFileMessage(Message<TUserCommand> message)
        {
            var type = message.GetData<Message<TUserCommand>.FileProgressMessageType>("type");

            var reply = message.GetReply(Message<TUserCommand>.GeneralMessageType.FileProgress);

            switch (type)
            {
                case Message<TUserCommand>.FileProgressMessageType.GetFile:
                    {
                        string fileName = message.GetData<string>("fileName");
                        SendFile(message.TokenFrom, fileName);
                    }
                    break;

                case Message<TUserCommand>.FileProgressMessageType.RecvFilesProgress:
                    {
                        var fileToken = message.GetData<string>("fileToken");

                        if (sendRecvController.TryGetSendingFile(message.TokenFrom, fileToken, out SendingFile file))
                        {
                            var next = file.GetNextPart();

                            if (next.Available != 0)
                            {
                                reply.Add("data", next)
                                     .Add("fileToken", fileToken)
                                     .Add("type", Message<TUserCommand>.FileProgressMessageType.SendFilesProgress);
                            }
                            else
                            {
                                sendRecvController.RemoveSendingFile(message.TokenFrom, fileToken);

                                reply.Add("fileToken", fileToken)
                                     .Add("type", Message<TUserCommand>.FileProgressMessageType.FileSended);
                            }
                        }
                        else
                        {
                            reply.Add("fileToken", fileToken)
                                 .Add("type", Message<TUserCommand>.FileProgressMessageType.FileNotExist);
                        }
                    }
                    break;

                case Message<TUserCommand>.FileProgressMessageType.FileNotExist:
                    {
                        return;
                    }

                case Message<TUserCommand>.FileProgressMessageType.FileSended:
                    {
                        var fileToken = message.GetData<string>("fileToken");

                        bool ok = sendRecvController.TryGetReceivingFile(message.TokenFrom, fileToken, out ReceivingFile file);
                        ok &= file?.IsWrited ?? false;

                        if (ok)
                        {
                            OnFileLoadProgress?.Invoke(file.ProgressData);
                        }
                        else
                        {
                            OnFileLoadProgress?.Invoke(new ProgressFileData
                            {
                                Name = message.GetData<string>("fileName"),
                                State = ProgressFileData.States.Error
                            });
                        }
                        sendRecvController.RemoveSendingFile(message.TokenFrom, fileToken);
                    }
                    return;

                case Message<TUserCommand>.FileProgressMessageType.SendFilesProgress:
                    {
                        var filePart = message.GetData<FilePart>("data");
                        string fileToken = message.GetData<string>("fileToken");

                        if (sendRecvController.TryGetReceivingFile(message.TokenFrom, fileToken, out ReceivingFile file))
                        {
                            file.AddBytes(filePart.Data, filePart.Available);

                            if (file.IsWrited)
                            {
                                sendRecvController.RemoveRecvingFile(message.TokenFrom, fileToken);
                            }

                            reply.Add("fileToken", fileToken)
                                 .Add("type", Message<TUserCommand>.FileProgressMessageType.RecvFilesProgress);
                        }
                    }
                    break;

                case Message<TUserCommand>.FileProgressMessageType.SendFile:
                    {
                        string fileName = message.GetData<string>("fileName");
                        var path = workPath + @"\" + fileName;
                        int fileSize = message.GetData<int>("totalSize");
                        string fileToken = message.GetData<string>("fileToken");

                        sendRecvController.AddRecvFile(message.TokenFrom, path, fileName, fileSize, fileToken);

                        reply.Add("fileToken", fileToken)
                             .Add("type", Message<TUserCommand>.FileProgressMessageType.RecvFilesProgress);
                    }
                    break;
            }

            SendMessage(reply);
        }

        private void UpdateAlive(string token)
        {
            if (connections.TryGetValue(token, out Connection connection))
            {
                connection.UpdateCloseTime();
            }
        }

        private AutoResetEvent RegNewToken(string token)
        {
            if (sendMessageQueue.ContainsKey(token) == false)
            {
                sendMessageQueue.TryAdd(token, new ConcurrentQueueWithSignal<Message<TUserCommand>>());
            }

            return sendMessageQueue[token].SignalEvent;
        }

        public void SendMessage(Message<TUserCommand> message)
        {
            if (sendMessageQueue.TryGetValue(message.TokenTo, out ConcurrentQueueWithSignal<Message<TUserCommand>> item))
            {
                item.Enqueue(message);
            }
        }

        private bool TryGetMessageForToken(string token, out Message<TUserCommand> message)
        {
            message = default;

            if (sendMessageQueue.TryGetValue(token, out ConcurrentQueueWithSignal<Message<TUserCommand>> item) == false)
            {
                return false;
            }

            if (item.IsEmpty == false)
            {
                return item.TryDequeue(out message);
            }
            else
            {
                if (connections.ContainsKey(token) == false)
                {
                    connections.TryAdd(token, new Connection(MyToken, token));
                }

                if (connections.TryGetValue(token, out Connection connection))
                {
                    if (DateTime.Now > connection.CloseConnectionTime)
                    {
                        throw new Exception("connection timeout");
                    }

                    if (DateTime.Now > connection.NextSendPingTime)
                    {
                        connection.UpdatePingTime();
                        message = connection.PingMessage;
                        return true;
                    }
                }
            }

            return false;
        }

        private void CheckRecvMessageThread()
        {
            while (true)
            {
                recvMessageQueue.Wait();
                while (recvMessageQueue.TryDequeue(out Message<TUserCommand> message))
                {
                    CheckRecvMessage(message);
                }
            }
        }

        private void CheckRecvMessage(Message<TUserCommand> message)
        {
            UpdateAlive(message.TokenFrom);

            if (message.MessageType == Message<TUserCommand>.GeneralMessageType.Close)
            {
                throw new Exception("close connection message");
            }

            if (message.MessageType != Message<TUserCommand>.GeneralMessageType.Ping
                && message.MessageType != Message<TUserCommand>.GeneralMessageType.FileProgress)
            {
                Log("recv: " + message.OrigData);
            }

            switch (message.MessageType)
            {
                case Message<TUserCommand>.GeneralMessageType.FileProgress:
                    CheckFileMessage(message);
                    break;

                case Message<TUserCommand>.GeneralMessageType.User:
                    OnGetMessage(message);
                    break;

                case Message<TUserCommand>.GeneralMessageType.SendReg:
                    if (isServer == true)
                    {
                        break;
                    }

                    var sendEvent = RegNewToken(message.TokenFrom);
                    Token.Value = message.GetData<string>("token");
                    InternalNewConnectAction?.Invoke(message.TokenFrom, mainSocket, sendEvent);
                    NewConnectAction?.Invoke(message.TokenFrom);
                    break;
            }
        }

        protected void SendThread(Socket socket, string token, AutoResetEvent sendEvent)
        {
            try
            {
                while (NeedStop == false)
                {
                    sendEvent.WaitOne(100);

                    if (TryGetMessageForToken(token, out Message<TUserCommand> message))
                    {
                        Utils.SendPackage(socket, message.GetJson());

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
                OnError(token, socket, ex);
            }
        }

        protected void RecvThread(Socket socket)
        {
            bool hasToken = false;

            try
            {
                while (NeedStop == false)
                {
                    var bytes = Utils.GetPackage(socket);
                    var data = Encoding.UTF32.GetString(bytes);
                    var messageFrom = Message<TUserCommand>.FromJson(data);

                    if (isServer
                        && hasToken == false
                        && messageFrom.MessageType == Message<TUserCommand>.GeneralMessageType.GetReg)
                    {
                        hasToken = true;
                        var token = TokenGenerator.Generate();

                        var ans = new Message<TUserCommand>(Token.Value, token, Message<TUserCommand>.GeneralMessageType.SendReg)
                            .Add("token", token);

                        var sendEvent = RegNewToken(token);

                        SendMessage(ans);
                        InternalNewConnectAction?.Invoke(token, socket, sendEvent);
                        NewConnectAction?.Invoke(token);
                    }

                    recvMessageQueue.Enqueue(messageFrom);
                }
            }
            catch (Exception ex)
            {
                OnError("", socket, ex);
            }
        }

        protected void OnError(string token, Socket socket, Exception ex)
        {
            socket?.Shutdown(SocketShutdown.Both);
            Log("error " + ex.ToString() + " \n" + ex.StackTrace);
            OnErrorAction?.Invoke(ex, token);
        }

        public void SetWorkPath(string path)
        {
            workPath = path;
        }

        public virtual bool Start()
        {
            return true;
        }

        public virtual void Stop()
        {
            sendRecvController.Dispose();
            _needStop.Value = true;
        }

        private void Log(string text)
        {
            Console.WriteLine((this is Client<TUserCommand> ? "[client] " : "[server] ") + text);
        }

    }
}
