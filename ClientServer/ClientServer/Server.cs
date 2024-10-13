using ClientServer.fileSend;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientServer
{

    public class Server<TUserCommand>
    {
        private readonly Socket sListener;
        private readonly IPEndPoint ipEndPoint;
        private bool needStop = false;

        private string workPath;

        public Action<Message<TUserCommand>> onGetMessage;

        private readonly Action<Exception, string> onErrorAction;

        Thread workThread;

        public delegate string GetFilePathDelegate(string name);
        public GetFilePathDelegate GetFilePath;

        private readonly SendRecvController sendRecvController;
        private readonly ConcurrentDictionary<string, ConcurrentQueue<Message<TUserCommand>>> messageQueue;

        public readonly string ServerToken = TokenGenerator.Generate();

        public Server(string ip, Action<Exception, string> onError, int port = Utils.defaultPort)
        {
            var ipAddr = IPAddress.Parse(ip);
            ipEndPoint = new IPEndPoint(ipAddr, port);

            sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            onErrorAction += onError;

            sendRecvController = new SendRecvController();
            messageQueue = new ConcurrentDictionary<string, ConcurrentQueue<Message<TUserCommand>>>();
        }

        public void SetWorkPath(string path)
        {
            workPath = path;
        }

        public void Start()
        {
            workThread = new Thread(Work);
            workThread.Start();
        }

        public void Stop()
        {
            sListener.Close();
            workThread.Abort();
            needStop = true;
        }

        private void CheckRecvMessage(Message<TUserCommand> message, ref bool isFirstMessage, ref string newToken)
        {
            isFirstMessage = false;
            if (message.MessageType == Message<TUserCommand>.GeneralMessageType.GetReg)
            {
                isFirstMessage = true;
                newToken = TokenGenerator.Generate();

                var ans = new Message<TUserCommand>(ServerToken, newToken, Message<TUserCommand>.GeneralMessageType.SendReg)
                    .Add("token", newToken)
                    .Add("name", message.GetData<string>("name"));

                message.Add("token", newToken);

                onGetMessage(message);
                SendMessage(ans);
            }
            else if (message.MessageType == Message<TUserCommand>.GeneralMessageType.GetFile)
            {
                string fileName = message.GetData<string>("fileName");
                string filePath = GetFilePath(fileName);

                if (File.Exists(filePath))
                {
                    var length = Utils.GetFileSize(filePath);
                    onGetMessage(message);

                    var fileToken = sendRecvController.AddSendFile(message.TokenFrom, filePath);

                    SendMessage(new Message<TUserCommand>(
                        ServerToken,
                        message.TokenFrom,
                        Message<TUserCommand>.GeneralMessageType.SendFile)
                            .Add("totalSize", length.ToString())
                            .Add("fileName", fileName)
                            .Add("fileToken", fileToken));
                }
                else
                {
                    SendMessage(new Message<TUserCommand>(
                        ServerToken,
                        message.TokenFrom,
                        Message<TUserCommand>.GeneralMessageType.FileNotExists));
                }
            }
            else if (message.MessageType == Message<TUserCommand>.GeneralMessageType.SendFile)
            {
                throw new NotImplementedException();
                //return new Message<TUserCommand>(Message<TUserCommand>.GeneralMessageType.Close);

                /*onGetMessage(message);

                string fileName = message.GetData("fileName");

                var path = workPath + @"\" + fileName;

                Utils.ReceiveFile(
                    path,
                    long.Parse(message.GetData("totalSize")),
                    socket,
                    null);


                var resMsg = new Message<TUserCommand>(Message<TUserCommand>.GeneralMessageType.FileRecived)
                        .Add("fileName", fileName)
                        .Add("filePath", path);

                onGetMessage(resMsg);

                return resMsg;*/
            }
            else if (message.MessageType == Message<TUserCommand>.GeneralMessageType.RecvFilesProgress)
            {
                var fileToken = message.GetData<string>("fileToken");

                if (sendRecvController.TryGetSendingFile(message.TokenFrom, fileToken, out SendingFile file))
                {
                    var next = file.GetNextPart();

                    if (next.Available != 0)
                    {
                        SendMessage(new Message<TUserCommand>(
                            ServerToken,
                            message.TokenFrom,
                            Message<TUserCommand>.GeneralMessageType.SendFilesProgress)
                                .Add("data", next)
                                .Add("fileToken", fileToken));
                    }
                    else
                    {
                        sendRecvController.RemoveSendingFile(message.TokenFrom, fileToken);

                        SendMessage(new Message<TUserCommand>(
                            ServerToken,
                            message.TokenFrom,
                            Message<TUserCommand>.GeneralMessageType.FileSended)
                                .Add("fileToken", fileToken));
                    }
                }
                else
                {
                    SendMessage(new Message<TUserCommand>(
                        ServerToken,
                        message.TokenFrom,
                        Message<TUserCommand>.GeneralMessageType.FileNotExists)
                            .Add("fileToken", fileToken));
                }
            }
            else
            {
                onGetMessage(message);
            }
        }

        private void SendThread(Socket handler, string token)
        {
            DateTime nextPingTime = DateTime.Now;
            Message<TUserCommand> pingMessage = default;

            try
            {
                while (needStop == false)
                {
                    if (messageQueue.TryGetValue(token, out ConcurrentQueue<Message<TUserCommand>> queue) == false)
                    {
                        throw new Exception("fail get queue");
                    }

                    if (queue.IsEmpty == false)
                    {
                        if (queue.TryDequeue(out Message<TUserCommand> message))
                        {
                            Utils.SendPackage(handler, message.GetJson());

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
                                ServerToken,
                                token,
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
            catch (Exception ex)
            {
                OnError(token, handler, ex);
            }
        }

        private void RecvThread(Socket handler)
        {
            string token = "";
            bool isFirstMessage = false;

            try
            {
                while (needStop == false)
                {
                    var bytes = Utils.GetPackage(handler);
                    var data = Encoding.UTF32.GetString(bytes);

                    var messageFrom = Message<TUserCommand>.FromJson(data);
                    if (messageFrom.MessageType != Message<TUserCommand>.GeneralMessageType.Ping
                        && messageFrom.MessageType != Message<TUserCommand>.GeneralMessageType.SendFilesProgress
                        && messageFrom.MessageType != Message<TUserCommand>.GeneralMessageType.RecvFilesProgress)
                    {
                        Log("recv: " + data);
                    }

                    CheckRecvMessage(messageFrom, ref isFirstMessage, ref token);
                    if (isFirstMessage)
                    {
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

        private void Work()
        {
            sListener.Bind(ipEndPoint);
            sListener.Listen(100);

            while (true)
            {
                Socket handler = sListener.Accept();
                new Task(() => RecvThread(handler)).Start();
            }
        }

        private void Log(string data)
        {
            Console.WriteLine(data);
        }

    }
}
