using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClientServer
{
    public class Message<TUserCommand>
    {
        public enum GeneralMessageType
        {
            User = 0,

            Ping = 1,
            Close = 2,

            GetReg = 3,
            SendReg = 4,

            FileProgress = 5,
        }

        public enum FileProgressMessageType
        {
            Error = 0,

            GetFile = 1,
            SendFile = 2,

            SendFilesProgress = 3,
            RecvFilesProgress = 4,

            FileReceived = 5,
            FileSended = 6,

            FileNotExist = 7,
        };

        [JsonPropertyName("command")]
        public TUserCommand Command { get; set; }

        [JsonPropertyName("messageType")]
        public GeneralMessageType MessageType { get; private set; }

        [JsonPropertyName("tokenFrom")]
        public string TokenFrom { get; private set; }

        [JsonPropertyName("tokenTo")]
        public string TokenTo { get; private set; }

        [JsonPropertyName("data")]
        public Dictionary<string, string> Data { get; set; }

        [JsonIgnore]
        public string OrigData { get; private set; }

        [JsonConstructor]
        public Message(string tokenFrom, string tokenTo, GeneralMessageType messageType = GeneralMessageType.User)
        {
            this.MessageType = messageType;
            Data = new Dictionary<string, string>();
            this.TokenFrom = tokenFrom;
            this.TokenTo = tokenTo;
        }

        public Message<TUserCommand> GetReply(GeneralMessageType generalMessageType = GeneralMessageType.User)
        {
            return new Message<TUserCommand>(TokenTo, TokenFrom, generalMessageType);
        }

        public string GetJson()
        {
            return JsonSerializer.Serialize(this, typeof(Message<TUserCommand>));
        }

        public static Message<TUserCommand> FromJson(string json)
        {
            var message = JsonSerializer.Deserialize<Message<TUserCommand>>(json);
            message.OrigData = json;
            return message;
        }

        public Message<TUserCommand> Add(string key, object value)
        {
            return Add(key, JsonSerializer.Serialize(value));
        }

        public Message<TUserCommand> Add(string key, string value)
        {
            Data[key] = value;
            return this;
        }

        public Message<TUserCommand> SetCommand(TUserCommand command)
        {
            Command = command;
            return this;
        }

        public T GetData<T>(string key)
        {
            if (Data.TryGetValue(key, out string value))
            {
                try
                {
                    if (typeof(T) == typeof(string))
                    {
                        value = (value.StartsWith("\"") ? "" : "\"") + value + (value.EndsWith("\"") ? "" : "\"");
                    }

                    return JsonSerializer.Deserialize<T>(value);
                }
                catch (Exception)
                {
                }
            }

            return default;
        }

        public void RemoveData(string key)
        {
            Data.Remove(key);
        }

    }
}
