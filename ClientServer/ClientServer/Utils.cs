using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        public void Wait()
        {
            SignalEvent.WaitOne(100);
        }
    }

    public static class Utils
    {
        private const int numCount = 10;

        public const int defaultPort = 35124;

        private static Task<int> ReceiveAsync(this Socket socket, byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            var tcs = new TaskCompletionSource<int>();
            socket.BeginReceive(buffer, offset, size, socketFlags, ar =>
            {
                try { tcs.TrySetResult(socket.EndReceive(ar)); }
                catch (Exception e) { tcs.TrySetException(e); }
            }, state: null);
            return tcs.Task;
        }

        public static async Task<byte[]> RecvCountAsync(Socket socket, int count)
        {
            byte[] ret = new byte[count];

            int totalRecv = 0;
            while (totalRecv != count)
            {
                totalRecv += await socket.ReceiveAsync(ret, totalRecv, ret.Length - totalRecv, SocketFlags.None);
            }
            return ret;
        }

        internal static async Task<byte[]> GetPackageAsync(Socket socket)
        {
            var recvBytes = await RecvCountAsync(socket, Utils.numCount);
            int needCount = int.Parse(Encoding.UTF8.GetString(recvBytes));

            return await RecvCountAsync(socket, needCount);
        }

        private static IList<ArraySegment<byte>> PrepareDataForSend(string data)
        {
            var bytes = Encoding.UTF32.GetBytes(AddChar(data, Utils.numCount));
            var preSendBytes = Encoding.UTF8.GetBytes(AddChar(bytes.Length.ToString(), Utils.numCount));

            return new List<ArraySegment<byte>> {
                new ArraySegment<byte>(preSendBytes),
                new ArraySegment<byte>(bytes)
            };
        }

        internal static void SendPackage(Socket socket, string data)
        {
            socket.Send(PrepareDataForSend(data));
        }

        internal static async Task SendPackageAsync(Socket socket, string data)
        {
            await socket.SendAsync(PrepareDataForSend(data), SocketFlags.None);
        }

        private static string AddChar(string str, int count)
        {
            while (str.Length < count)
                str = '0' + str;

            return str;
        }

    }
}
