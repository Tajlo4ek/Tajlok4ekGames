using ClientServer.FileUtils;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientServer
{
    public abstract class BaseClientServer<TUserCommand>
    {
        protected readonly Tajlo4ekUtils.AtomicValue<string> Token = new Tajlo4ekUtils.AtomicValue<string>("");
        protected readonly Tajlo4ekUtils.AtomicValue<bool> _needStop = new Tajlo4ekUtils.AtomicValue<bool>(false);

        private bool NeedStop { get { return _needStop.Value; } }

        public string MyToken { get { return Token.Value; } }

        protected readonly IPEndPoint ipEndPoint;
        protected readonly Socket mainSocket;

        public Action<Exception, string> OnErrorAction;
        private readonly SendRecvController sendRecvController;

        public Action<Message<TUserCommand>> OnGetMessage;

        protected Action<string, Socket> InternalNewConnectAction;
        public Action<string> NewConnectAction;

        public Action<ProgressFileData> OnFileLoadProgress
        {
            get { return sendRecvController.OnLoadCallback; }
            set { sendRecvController.OnLoadCallback = value; }
        }

        private string workPath;
        private readonly bool isServer;

        private readonly AsyncQueue<Message<TUserCommand>> recvMessageQueue;
        private readonly ConcurrentDictionary<string, AsyncQueue<Message<TUserCommand>>> sendMessageQueue;
        private readonly ConcurrentDictionary<CancellationTokenSource, Task> tasks;

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


            tasks = new ConcurrentDictionary<CancellationTokenSource, Task>();
            sendMessageQueue = new ConcurrentDictionary<string, AsyncQueue<Message<TUserCommand>>>();
            sendRecvController = new SendRecvController();

            connections = new ConcurrentDictionary<string, Connection>();
            recvMessageQueue = new AsyncQueue<Message<TUserCommand>>();
            this.isServer = isServer;


            RegNewTask(async () => await CheckRecvMessageAsync());
            RegNewTask(async () => await PingWorkAsync());
        }

        protected void RegNewTask(Func<Task> task)
        {
            var cancelToken = new CancellationTokenSource();
            tasks.TryAdd(cancelToken, Task.Run(task, cancelToken.Token));
        }

        public void SendFile(string tokenTo, string fileName)
        {
            var message = new Message<TUserCommand>(MyToken, tokenTo, Message<TUserCommand>.GeneralMessageType.FileProgress);

            var path = workPath + @"\" + fileName;

            if (File.Exists(path))
            {
                var length = Tajlo4ekUtils.FileUtils.GetFileSize(path);

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
                    return;

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

                            if (file.Sended)
                            {
                                sendRecvController.RemoveSendingFile(message.TokenFrom, fileToken);
                            }
                        }
                        else
                        {
                            reply.Add("fileToken", fileToken)
                                 .Add("type", Message<TUserCommand>.FileProgressMessageType.FileNotExist);
                        }

                        SendMessage(reply);
                    }
                    return;

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
                        sendRecvController.RemoveRecvingFile(message.TokenFrom, fileToken);
                    }
                    return;

                case Message<TUserCommand>.FileProgressMessageType.SendFilesProgress:
                    {
                        var filePart = message.GetData<FilePart>("data");
                        string fileToken = message.GetData<string>("fileToken");

                        if (sendRecvController.TryGetReceivingFile(message.TokenFrom, fileToken, out ReceivingFile file))
                        {
                            file.AddBytes(filePart.Data, filePart.Available);

                            reply.Add("fileToken", fileToken)
                                 .Add("type", Message<TUserCommand>.FileProgressMessageType.RecvFilesProgress);
                        }
                        SendMessage(reply);
                    }
                    return;

                case Message<TUserCommand>.FileProgressMessageType.SendFile:
                    {
                        string fileName = message.GetData<string>("fileName");
                        var path = workPath + @"\" + fileName;
                        int fileSize = message.GetData<int>("totalSize");
                        string fileToken = message.GetData<string>("fileToken");

                        sendRecvController.AddRecvFile(message.TokenFrom, path, fileName, fileSize, fileToken);

                        reply.Add("fileToken", fileToken)
                             .Add("type", Message<TUserCommand>.FileProgressMessageType.RecvFilesProgress);

                        SendMessage(reply);
                    }
                    return;
            }

        }

        private void UpdateAlive(string token)
        {
            if (connections.TryGetValue(token, out Connection connection))
            {
                connection.UpdateCloseTime();
            }
        }

        private void RegNewToken(string token)
        {
            if (sendMessageQueue.ContainsKey(token) == false)
            {
                sendMessageQueue.TryAdd(token, new AsyncQueue<Message<TUserCommand>>());
            }

            connections.TryAdd(token, new Connection(MyToken, token));
        }

        public void SendMessage(Message<TUserCommand> message)
        {
            if (sendMessageQueue.TryGetValue(message.TokenTo, out AsyncQueue<Message<TUserCommand>> item))
            {
                if (item.IsCompleted == false)
                {
                    item.Enqueue(message);
                }
            }
        }

        private async Task PingWorkAsync()
        {
            while (NeedStop == false)
            {
                foreach (var connection in connections)
                {
                    var token = connection.Key;
                    var value = connection.Value;

                    if (DateTime.Now > value.NextSendPingTime)
                    {
                        value.UpdatePingTime();
                        SendMessage(value.PingMessage);
                    }

                    if (DateTime.Now > value.CloseConnectionTime)
                    {
                        //TODO: remove token from all
                        sendMessageQueue[token].Complete();
                    }
                }

                await Task.Delay(1000);
            }
        }

        private async Task CheckRecvMessageAsync()
        {
            try
            {
                while (true)
                {
                    var message = await recvMessageQueue.DequeueAsync();
                    CheckRecvMessage(message);
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        private void CheckRecvMessage(Message<TUserCommand> message)
        {
            UpdateAlive(message.TokenFrom);
#if DEBUG
            Log(message, true);
#endif

            if (message.MessageType == Message<TUserCommand>.GeneralMessageType.Close)
            {
                throw new Exception("close connection message");
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

                    Token.Value = message.GetData<string>("token");
                    RegNewToken(message.TokenFrom);
                    InternalNewConnectAction?.Invoke(message.TokenFrom, mainSocket);
                    NewConnectAction?.Invoke(message.TokenFrom);
                    break;
            }
        }

        protected async Task SendThreadAsync(Socket socket, string token)
        {
            var queue = sendMessageQueue[token];

            try
            {
                while (NeedStop == false)
                {
                    var message = await queue.DequeueAsync();
                    await Utils.SendPackageAsync(socket, message.GetJson());
#if DEBUG
                    Log(message, false);
#endif
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("ss:fff"));
                OnError(token, socket, ex);
            }
        }

        protected async Task RecvThreadAsync(Socket socket)
        {
            bool hasToken = false;

            try
            {
                while (NeedStop == false)
                {
                    var bytes = await Utils.GetPackageAsync(socket);
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

                        RegNewToken(token);

                        SendMessage(ans);
                        InternalNewConnectAction?.Invoke(token, socket);
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
#if DEBUG
            Log("error " + ex.ToString() + " \n" + ex.StackTrace);
#endif
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

            recvMessageQueue.Complete();

            foreach (var task in tasks)
            {
                task.Key.Cancel();
            }
        }

#if DEBUG

        private void Log(Message<TUserCommand> message, bool recv)
        {
            if (message.MessageType == Message<TUserCommand>.GeneralMessageType.Ping
               /* || message.MessageType == Message<TUserCommand>.GeneralMessageType.FileProgress*/)
            {
                return;
            }

            Log((recv ? "recv " : "send ") + message.GetJson());
        }

        private void Log(string text)
        {
            Console.WriteLine((this is Client<TUserCommand> ? "[client] " : "[server] ") + text);
        }
#endif

    }
}
