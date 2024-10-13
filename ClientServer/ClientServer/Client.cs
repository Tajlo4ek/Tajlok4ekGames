using ClientServer.fileSend;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace ClientServer
{
    public class Client<TUserCommand>
    {
        private readonly Tajlo4ekUtils.AtomicValue<string> Token = new Tajlo4ekUtils.AtomicValue<string>();
        private readonly Tajlo4ekUtils.AtomicValue<string> serverToken = new Tajlo4ekUtils.AtomicValue<string>();

        private string workPath;
        private readonly string name;

        public Action<Message<TUserCommand>> OnGetMessage;

        private readonly Action<Exception> onErrorAction;

        public Action<ProgressFileData> OnFileLoadProgress;

        Thread workSendThread;
        Thread workRecvThread;

        public delegate string GetFilePathDelegate(string name);
        public GetFilePathDelegate GetFilePath;

        private bool needStop = false;
        private readonly Socket sender;

        private readonly SendRecvController sendRecvController;

        private readonly ConcurrentQueue<Message<TUserCommand>> messageQueue;

        public Client(string ip, string name, Action<Exception> onError, int port = Utils.defaultPort)
        {
            var ipAddr = IPAddress.Parse(ip);
            var ipEndPoint = new IPEndPoint(ipAddr, port);
            sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(ipEndPoint);

            this.onErrorAction += onError;
            Token.Value = "";
            serverToken.Value = "";

            messageQueue = new ConcurrentQueue<Message<TUserCommand>>();
            sendRecvController = new SendRecvController();

            this.name = name;
        }

        public void Start()
        {
            needStop = false;
            workSendThread = new Thread(SendThread);
            workRecvThread = new Thread(RecvThread);

            workSendThread.Start();
            workRecvThread.Start();
        }

        public void Stop()
        {
            needStop = true;
        }

        public void SetWorkPath(string path)
        {
            workPath = path;
        }

        private void SendThread()
        {
            var getRegTimeout = DateTime.Now.AddSeconds(30);

            DateTime nextPingTime = DateTime.Now;
            Message<TUserCommand> pingMessage = default;

            bool isSendStart = false;

            try
            {
                while (needStop == false)
                {
                    if (Token.Value.Length == 0)
                    {
                        if (isSendStart == false)
                        {
                            isSendStart = true;

                            var startMessage = new Message<TUserCommand>("", "", Message<TUserCommand>.GeneralMessageType.GetReg)
                                .Add("name", name);

                            Utils.SendPackage(sender, startMessage.GetJson());
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
                        if (messageQueue.IsEmpty == false)
                        {
                            if (messageQueue.TryDequeue(out Message<TUserCommand> message))
                            {
                                Utils.SendPackage(sender, message.GetJson());

                                if (message.MessageType != Message<TUserCommand>.GeneralMessageType.Ping
                                    && message.MessageType != Message<TUserCommand>.GeneralMessageType.SendFilesProgress
                                    && message.MessageType != Message<TUserCommand>.GeneralMessageType.RecvFilesProgress)
                                {
                                    Log("send: " + message.GetJson());
                                }
                            }
                        }
                        else if (nextPingTime < DateTime.Now)
                        {
                            nextPingTime = DateTime.Now.AddSeconds(1);
                            if (pingMessage == default)
                            {
                                pingMessage = new Message<TUserCommand>(
                                    Token.Value,
                                    serverToken.Value,
                                    Message<TUserCommand>.GeneralMessageType.Ping);
                            }
                            SendMessage(pingMessage);
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
                while (needStop == false)
                {
                    var bytes = Utils.GetPackage(sender);
                    var data = Encoding.UTF32.GetString(bytes);

                    var messageFrom = Message<TUserCommand>.FromJson(data);

                    if (messageFrom.MessageType != Message<TUserCommand>.GeneralMessageType.Ping
                        && messageFrom.MessageType != Message<TUserCommand>.GeneralMessageType.SendFilesProgress
                        && messageFrom.MessageType != Message<TUserCommand>.GeneralMessageType.RecvFilesProgress)
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

            needStop = true;
            onErrorAction(ex);

            workRecvThread.Abort();
            workSendThread.Abort();

            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        private void CheckRecvMessage(Message<TUserCommand> message)
        {
            switch (message.MessageType)
            {
                case Message<TUserCommand>.GeneralMessageType.SendReg:
                    {
                        serverToken.Value = message.TokenFrom;
                        Token.Value = message.GetData<string>("token");
                    }
                    break;

                case Message<TUserCommand>.GeneralMessageType.SendFile:
                    {
                        string fileName = message.GetData<string>("fileName");
                        var path = workPath + @"\" + fileName;
                        int fileSize = message.GetData<int>("totalSize");
                        string fileToken = message.GetData<string>("fileToken");

                        sendRecvController.AddRecvFile(message.TokenFrom, path, fileName, fileSize, fileToken);

                        SendMessage(new Message<TUserCommand>(
                            Token.Value,
                            serverToken.Value,
                            Message<TUserCommand>.GeneralMessageType.RecvFilesProgress)
                                .Add("fileToken", fileToken));
                    }
                    break;

                case Message<TUserCommand>.GeneralMessageType.SendFilesProgress:
                    {
                        var filePart = message.GetData<FilePart>("data");
                        string fileToken = message.GetData<string>("fileToken");

                        if (sendRecvController.TryGetReceivingFile(message.TokenFrom, fileToken, out ReceivingFile file))
                        {
                            file.AddBytes(filePart.Data, filePart.Available);
                            SendMessage(new Message<TUserCommand>(
                                Token.Value,
                                serverToken.Value,
                                Message<TUserCommand>.GeneralMessageType.RecvFilesProgress)
                                .Add("fileToken", fileToken));
                        }
                    }
                    break;

                case Message<TUserCommand>.GeneralMessageType.FileSended:
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
            }

            OnGetMessage(message);
        }

        public void SendMessage(Message<TUserCommand> message)
        {
            messageQueue.Enqueue(message);
        }

        private void Log(string data)
        {
            Console.WriteLine(data);
        }

    }
}


