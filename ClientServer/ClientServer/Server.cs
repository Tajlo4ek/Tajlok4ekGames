using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientServer
{
    public class Server<TUserCommand>
    {
        private const int tokenSize = 20;

        public static string ServerToken = "server";

        private readonly Socket sListener;
        private readonly IPEndPoint ipEndPoint;

        private readonly StreamWriter outStream;

        private string workPath;

        public Action<Message<TUserCommand>> onGetMessage;

        private readonly Action<Exception, string> onErrorAction;

        public delegate Message<TUserCommand> GetMessageDelegate(string token);
        private readonly GetMessageDelegate getMessageForUser;

        Thread workThread;

        public delegate string GetFilePathDelegate(string name);
        public GetFilePathDelegate GetFilePath;

        public Server(string ip, GetMessageDelegate getMessage, Action<Exception, string> onError)
        {
            var ipAddr = IPAddress.Parse(ip);
            ipEndPoint = new IPEndPoint(ipAddr, Utils.port);

            sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            outStream = new StreamWriter("server.log");

            getMessageForUser += getMessage;
            onErrorAction += onError;
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
            outStream.Flush();
            outStream.Close();
        }

        private Message<TUserCommand> CheckMessage(Message<TUserCommand> message, Socket socket)
        {

            if (message.MessageType == Message<TUserCommand>.GeneralMessageType.GetReg)
            {
                var token = TokenGenerator.Generate(tokenSize);

                var ans = new Message<TUserCommand>(Message<TUserCommand>.GeneralMessageType.SendReg)
                    .SetToken(ServerToken)
                    .Add("token", token)
                    .Add("name", message.GetData("name"));

                message.Token = token;
                onGetMessage(message);
                return ans;
            }
            else if (message.MessageType == Message<TUserCommand>.GeneralMessageType.GetFile)
            {
                string fileName = message.GetData("fileName");
                string filePath = GetFilePath(fileName);

                if (File.Exists(filePath))
                {
                    var length = Utils.GetFileSize(filePath);
                    onGetMessage(message);


                    var ans = new Message<TUserCommand>(Message<TUserCommand>.GeneralMessageType.SendFile)
                        .SetToken(ServerToken)
                        .Add("totalSize", length.ToString())
                        .Add("fileName", fileName)
                        .Add("filePath", filePath);

                    return ans;
                }
                else
                {
                    var ans = new Message<TUserCommand>(Message<TUserCommand>.GeneralMessageType.FileNotExists)
                        .SetToken(ServerToken);

                    return ans;
                }
            }
            else if (message.MessageType == Message<TUserCommand>.GeneralMessageType.SendFile)
            {
                onGetMessage(message);

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

                return resMsg;
            }
            else
            {
                onGetMessage(message);
                return getMessageForUser(message.Token);
            }
        }

        private void WorkWithConnect(Socket handler)
        {
            string token = "";

            while (true)
            {
                try
                {
                    var bytes = Utils.GetPackage(handler);
                    string data = Encoding.UTF32.GetString(bytes);
                    Log(data);
                    var messageFrom = Message<TUserCommand>.FromJson(data);


                    var reply = CheckMessage(messageFrom, handler);

                    string filePath = reply.GetData("filePath");
                    reply.RemoveData("filePath");

                    Utils.SendPackage(handler, reply.GetJson());

                    if (token.Length == 0)
                    {
                        token = messageFrom.Token;
                    }

                    if (reply.MessageType == Message<TUserCommand>.GeneralMessageType.SendFile)
                    {
                        Utils.SendFile(filePath, handler);

                        var ans = new Message<TUserCommand>(Message<TUserCommand>.GeneralMessageType.FileSended)
                            .SetToken(messageFrom.Token);

                        onGetMessage(ans);
                    }

                    if (reply.MessageType == Message<TUserCommand>.GeneralMessageType.FileSended)
                    {
                        Thread.Sleep(250);
                    }

                }
                catch (Exception ex)
                {

                    handler.Shutdown(SocketShutdown.Both);
                    Log("error " + ex.ToString() + " \n" + ex.StackTrace);
                    onErrorAction(ex, token);

                    break;
                }
            }
        }

        private void Work()
        {
            sListener.Bind(ipEndPoint);
            sListener.Listen(100);

            while (true)
            {
                Socket handler = sListener.Accept();
                new Task(() => WorkWithConnect(handler)).Start();
            }
        }

        private void Log(string data)
        {
            Console.WriteLine(data);

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

        }

    }
}
