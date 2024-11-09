using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientServer
{

    public class Server<TUserCommand> : BaseClientServer<TUserCommand>
    {
        public Server(IPAddress ip, int port = Utils.defaultPort)
            : base(ip, port, true)
        {
            Token.Value = TokenGenerator.Generate();

            InternalNewConnectAction = (token, socket) =>
            {
                RegNewTask(async () => await SendThreadAsync(socket, token));
            };
        }

        public override bool Start()
        {
            try
            {
                base.Start();

                mainSocket.Bind(ipEndPoint);
                mainSocket.Listen(100);
                AcceptCallback(null);
            }
            catch (Exception ex)
            {
                OnError("", null, ex);
                return false;
            }
            return true;
        }

        public override void Stop()
        {
            base.Stop();
            mainSocket?.Shutdown(SocketShutdown.Both);
            mainSocket?.Close();
        }

        private void AcceptCallback(IAsyncResult result)
        {
            if (result != null)
            {
                Socket handler = mainSocket.EndAccept(result);
                RegNewTask(async () => await RecvThreadAsync(handler));
            }

            mainSocket.BeginAccept(AcceptCallback, null);
        }
    }
}
