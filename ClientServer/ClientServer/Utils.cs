using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ClientServer
{
    public class ConcurrentQueueWithSignal<T>
    {
        public AutoResetEvent SignalEvent { get; private set; } = new AutoResetEvent(false);

        private readonly ConcurrentQueue<T> queue = new ConcurrentQueue<T>();

        public void Enqueue(T message)
        {
            queue.Enqueue(message);
            SignalEvent.Set();
        }

        public bool IsEmpty { get { return queue.IsEmpty; } }

        public bool TryDequeue(out T data)
        {
            return queue.TryDequeue(out data);
        }
    }

    public static class Utils
    {
        private const int numCount = 10;

        public const int defaultPort = 35124;

        private static byte[] RecvCount(Socket socket, int count, int timeout)
        {
            socket.ReceiveTimeout = timeout;

            var buffer = new byte[count];
            int totalRecv = 0;

            while (totalRecv != count)
            {
                var recvCount = socket.Receive(buffer, totalRecv, count - totalRecv, SocketFlags.None);
                totalRecv += recvCount;
            }

            return buffer;
        }

        internal static byte[] GetPackage(Socket socket, int timeout = 5000)
        {
            var recvBytes = RecvCount(socket, Utils.numCount, timeout);
            int needCount = int.Parse(Encoding.UTF8.GetString(recvBytes));

            return RecvCount(socket, needCount, timeout);
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
