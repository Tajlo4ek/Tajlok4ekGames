using System;
using System.Collections.Generic;
using static LauncherUtils.Messages;

namespace LauncherUtils
{
    public class Connection
    {

        public readonly string Token;

        private readonly Queue<ClientServer.Message<MessageType>> messages;

        private readonly ClientServer.Message<MessageType> pingMessage;

        private DateTime lastUpdateTime = DateTime.Now;

        private const int maxNotActiveTime = 5 * 60 * 1000;

        public Connection(string token, bool isServer)
        {
            this.Token = token;
            messages = new Queue<ClientServer.Message<Messages.MessageType>>();

            pingMessage = new ClientServer.Message<MessageType>(
                ClientServer.Message<MessageType>.GeneralMessageType.Ping)
                   .SetToken(Token);

            if (!isServer)
            {
                var message = new ClientServer.Message<MessageType>(ClientServer.Message<MessageType>.GeneralMessageType.User)
                    .SetCommand(MessageType.GetInfo)
                    .SetToken(Token);

                AddDataToSend(message);
            }
        }

        public void AddDataToSend(ClientServer.Message<MessageType> message)
        {
            lock (messages)
            {
                messages.Enqueue(message);
            }
        }

        public ClientServer.Message<MessageType> GetMessage()
        {
            if (messages.Count > 0)
            {
                var message = messages.Dequeue();
                return message;
            }
            return pingMessage;
        }

        public void Update()
        {
            lastUpdateTime = DateTime.Now;
        }

        public bool IsActive
        {
            get
            {
                return (DateTime.Now - lastUpdateTime).TotalMilliseconds < maxNotActiveTime;
            }
        }
    }
}
