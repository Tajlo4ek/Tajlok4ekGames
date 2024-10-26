using ClientServer.FileUtils;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;

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

        public Action<Exception, string> onErrorAction;
        private readonly SendRecvController sendRecvController;

        public Action<Message<TUserCommand>> onGetMessage;
        public Action<ProgressFileData> OnFileLoadProgress;

        private string workPath;

        private readonly ConcurrentDictionary<string, ConcurrentQueue<Message<TUserCommand>>> messageQueue;

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
                this.CloseConnectionTime = DateTime.Now.AddSeconds(5);
            }

            public void UpdatePingTime()
            {
                this.NextSendPingTime = DateTime.Now.AddSeconds(1);
            }
        }

        private readonly ConcurrentDictionary<string, Connection> connections;

        public BaseClientServer(IPAddress ipAddr, int port)
        {
            ipEndPoint = new IPEndPoint(ipAddr, port);
            mainSocket = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            messageQueue = new ConcurrentDictionary<string, ConcurrentQueue<Message<TUserCommand>>>();
            sendRecvController = new SendRecvController();

            connections = new ConcurrentDictionary<string, Connection>();
        }

        protected void CheckFileMessage(Message<TUserCommand> message)
        {
            var type = message.GetData<Message<TUserCommand>.FileProgressMessageType>("type");

            var reply = message.GetReply(Message<TUserCommand>.GeneralMessageType.FileProgress);

            switch (type)
            {
                case Message<TUserCommand>.FileProgressMessageType.GetFile:
                    {
                        string fileName = message.GetData<string>("fileName");
                        var path = workPath + @"\" + fileName;

                        if (File.Exists(path))
                        {
                            var length = Utils.GetFileSize(path);
                            onGetMessage(message);

                            var fileToken = sendRecvController.AddSendFile(message.TokenFrom, path);

                            reply.Add("totalSize", length.ToString())
                                 .Add("fileName", fileName)
                                 .Add("fileToken", fileToken)
                                 .Add("type", Message<TUserCommand>.FileProgressMessageType.SendFile);
                        }
                        else
                        {
                            reply.Add("type", Message<TUserCommand>.FileProgressMessageType.FileNotExists);
                        }
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
                                 .Add("type", Message<TUserCommand>.FileProgressMessageType.FileNotExists);
                        }
                    }
                    break;

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
                    break;

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

        protected void UpdateAlive(string token)
        {
            if (connections.TryGetValue(token, out Connection connection))
            {
                connection.UpdateCloseTime();
            }
        }

        public void SendMessage(Message<TUserCommand> message)
        {
            var token = message.TokenTo;

            if (messageQueue.ContainsKey(token) == false)
            {
                messageQueue.TryAdd(token, new ConcurrentQueue<Message<TUserCommand>>());
            }

            if (messageQueue.TryGetValue(token, out ConcurrentQueue<Message<TUserCommand>> queue))
            {
                queue.Enqueue(message);
            }
        }

        protected bool TryGetMessageForToken(string token, out Message<TUserCommand> message)
        {
            message = default;

            if (messageQueue.TryGetValue(token, out ConcurrentQueue<Message<TUserCommand>> queue) == false)
            {
                return false;
            }

            if (queue.IsEmpty == false)
            {
                return queue.TryDequeue(out message);
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

        protected virtual void CheckRecvMessage(Message<TUserCommand> message)
        {
            UpdateAlive(message.TokenFrom);

            if (message.MessageType == Message<TUserCommand>.GeneralMessageType.Close)
            {
                throw new Exception("close connection message");
            }
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
            _needStop.Value = true;
        }
    }
}
