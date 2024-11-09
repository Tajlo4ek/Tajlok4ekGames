using Microsoft.VisualStudio.Threading;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientServer
{
    public class Client<TUserCommand> : BaseClientServer<TUserCommand>
    {
        private readonly Tajlo4ekUtils.AtomicValue<string> serverToken = new Tajlo4ekUtils.AtomicValue<string>("");

        public string ServerToken { get { return serverToken.Value; } }

        public Client(IPAddress ip, int port = Utils.defaultPort)
            : base(ip, port, false)
        {
            InternalNewConnectAction += (token, socket) =>
            {
                serverToken.Value = token;
                RegNewTask(async () => await SendThreadAsync(socket, token));
            };
        }

        public override bool Start()
        {
            try
            {
                base.Start();
                mainSocket.Connect(ipEndPoint);

                RegNewTask(async () => await RecvThreadAsync(mainSocket));


                var startMessage = new Message<TUserCommand>("", "", Message<TUserCommand>.GeneralMessageType.GetReg);
                Utils.SendPackage(mainSocket, startMessage.GetJson());
            }
            catch (Exception ex)
            {
                OnError("", null, ex);
                return false;
            }
            return true;
        }
    }
}


