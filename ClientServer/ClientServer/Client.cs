
// #define NEED_LOG

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

#if NEED_LOG
using System.Threading.Tasks;
#endif

namespace ClientServer
{
    public class Client<TUserCommand>
    {

        public string Token { get; private set; }
        private readonly string name;

        private string workPath;

#if NEED_LOG
        static int id = 0;
        private readonly StreamWriter outStream;
#endif

        public Action<Message<TUserCommand>> OnGetMessage;

        public delegate Message<TUserCommand> GetMessageDelegate(string token);
        public GetMessageDelegate GetMessageForServer;

        private readonly Action<Exception> onErrorAction;

        public Action<string> OnFileLoadProcess;

        Thread workThread;

        public delegate string GetFilePathDelegate(string name);
        public GetFilePathDelegate GetFilePath;

        private bool needStop = false;
        private readonly Socket sender;

        public Client(string ip, string name, Action<Exception> onError, int port = Utils.defaultPort)
        {
            var ipAddr = IPAddress.Parse(ip);
            var ipEndPoint = new IPEndPoint(ipAddr, port);
            sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(ipEndPoint);

            this.name = name;
            this.onErrorAction += onError;
            Token = "";
#if NEED_LOG
            outStream = new StreamWriter("client" + id + ".log");
            id++;
#endif
        }

        public void Start()
        {
            needStop = false;
            workThread = new Thread(Work);
            workThread.Start();
        }

        public void Stop()
        {
            needStop = true;
#if NEED_LOG
            if (!(outStream.BaseStream == null))
            {
                outStream.Flush();
                outStream.Close();
            }
#endif
        }

        public void SetWorkPath(string path)
        {
            workPath = path;
        }

        public void Work()
        {
            try
            {
                while (true)
                {
                    if (Token == "")
                    {
                        var message = new Message<TUserCommand>(Message<TUserCommand>.GeneralMessageType.GetReg)
                            .Add("name", name);
                        SendMessage(message);
                    }
                    else
                    {
                        Thread.Sleep(5);
                        var message = GetMessageForServer(Token);
                        SendMessage(message);
                    }

                    if (needStop)
                    {
                        break;
                    }

                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString() + "\n" + ex.StackTrace);

                onErrorAction(ex);
                workThread.Abort();
            }

            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        private void OnEnd(Message<TUserCommand> message, Socket socket)
        {
            try
            {
                if (message.MessageType == Message<TUserCommand>.GeneralMessageType.SendReg)
                {
                    Token = message.GetData("token");
                    OnGetMessage(message);
                }
                else if (message.MessageType == Message<TUserCommand>.GeneralMessageType.SendFile)
                {
                    string fileName = message.GetData("fileName");

                    var path = workPath + @"\" + fileName;

                    OnFileLoadProcess?.Invoke(fileName);

                    Utils.ReceiveFile(
                        path,
                        long.Parse(message.GetData("totalSize")),
                        socket,
                        OnFileLoadProcess);

                    var resMsg = new Message<TUserCommand>(Message<TUserCommand>.GeneralMessageType.FileRecived)
                        .Add("fileName", fileName)
                        .Add("filePath", path);

                    OnGetMessage(resMsg);
                }
                else
                {
                    OnGetMessage(message);
                }

            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                onErrorAction(ex);
            }
        }

        private void SendMessage(Message<TUserCommand> message)
        {
            if (message.MessageType == Message<TUserCommand>.GeneralMessageType.SendFile)
            {
                var filePath = GetFilePath(message.GetData("fileName"));

                if (File.Exists(filePath))
                {
                    message.Add("totalSize", Utils.GetFileSize(filePath).ToString());
                    Utils.SendPackage(sender, message.GetJson());
                    Utils.SendFile(filePath, sender);
                }
                else
                {
                    var ans = new Message<TUserCommand>(Message<TUserCommand>.GeneralMessageType.FileNotExists)
                        .SetToken(this.Token);

                    Utils.SendPackage(sender, ans.GetJson());
                }
            }
            else
            {
                Utils.SendPackage(sender, message.GetJson());
            }

            var bytes = Utils.GetPackage(sender);
            var data = Encoding.UTF32.GetString(bytes);

            var messageFrom = Message<TUserCommand>.FromJson(data);


            OnEnd(messageFrom, sender);
        }

        private void Log(string data)
        {
            Console.WriteLine("recv:" + data);

#if NEED_LOG
            new Task(new Action(() =>
            {
                lock (outStream)
                {
                    if (outStream.BaseStream != null)
                    {
                        outStream.Write((int)(DateTime.Now - Utils.startTime).TotalMilliseconds);
                        outStream.Write(" ");
                        outStream.WriteLine(data);
                        outStream.Flush();
                    }
                }
            })).Start();
#endif
        }

    }
}


