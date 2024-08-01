using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ClientServer
{
    static class Utils
    {
        public static readonly DateTime startTime = DateTime.Now;

        public const int numCount = 10;

        public const int port = 35124;

        private const int maxPackageSize = 16384;

        public static void WaitCount(Socket socket, int count, int timeout)
        {
            int time = 0;

            while (socket.Available < count)
            {
                Thread.Sleep(10);

                time += 10;
                if (time >= timeout)
                    throw new Exception("timeout wait");
            }
        }

        public static byte[] GetPackage(Socket socket, int timeout = 5000)
        {
            WaitCount(socket, Utils.numCount, timeout);
            byte[] bytes = new byte[Utils.numCount];
            socket.Receive(bytes);
            int needCount = int.Parse(Encoding.UTF8.GetString(bytes));

            WaitCount(socket, needCount, timeout);
            bytes = new byte[needCount];
            socket.Receive(bytes);

            return bytes;
        }

        public static void SendPackage(Socket socket, string data)
        {
            var bytes = Encoding.UTF32.GetBytes(AddChar(data, Utils.numCount));
            socket.Send(Encoding.UTF8.GetBytes(AddChar(bytes.Length.ToString(), Utils.numCount)));
            socket.Send(bytes);
        }

        private static string AddChar(string str, int count)
        {
            while (str.Length < count)
                str = '0' + str;

            return str;
        }

        public static void SendFile(string path, Socket socket)
        {
            Thread.Sleep(1000);

            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                long lenght = file.Length;
                int nowSended = 0;

                while (nowSended < lenght)
                {
                    byte[] data = new byte[maxPackageSize];

                    int countRead = file.Read(data, 0, maxPackageSize);
                    socket.Send(Encoding.UTF8.GetBytes(Utils.AddChar(countRead.ToString(), Utils.numCount)));
                    socket.Send(data, countRead, SocketFlags.None);
                    nowSended += countRead;
                }
            }
        }

        public static void ReceiveFile(string path, long size, Socket socket, Action<string> onLoadCallaback)
        {
            long lenght = size;
            long nowSize = 0;

            int proc = 0;

            using (var file = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                while (nowSize < lenght)
                {
                    var bytes = Utils.GetPackage(socket, 30000);
                    file.Write(bytes, 0, bytes.Length);

                    nowSize += bytes.Length;

                    int buf = (int)(nowSize * 100 / lenght);
                    if (buf > proc)
                    {
                        proc = buf;
                        onLoadCallaback?.Invoke(proc.ToString() + "%");
                    }
                }
            }
        }

        public static long GetFileSize(string path)
        {
            return new FileInfo(path).Length;
        }


    }
}
