using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ClientServer
{

    public static class Utils
    {
        private const int numCount = 10;

        public const int defaultPort = 35124;


        private static void WaitCount(Socket socket, int count, int timeout)
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

        internal static byte[] GetPackage(Socket socket, int timeout = 5000)
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

        internal static void SendPackage(Socket socket, string data)
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

        internal static long GetFileSize(string path)
        {
            return new FileInfo(path).Length;
        }


    }
}
