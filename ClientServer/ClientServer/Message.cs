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

            GetReg = 2,
            SendReg = 3,

            GetFile = 4,
            SendFile = 5,
            SendFilesProgress = 6,
            RecvFilesProgress = 7,

            FileNotExists = 8,
            FileRecived = 9,
            FileSended = 10,

            Close = 11,
        }

        [JsonPropertyName("command")]
        public TUserCommand Command { get; set; }

        [JsonPropertyName("message")]
        public GeneralMessageType MessageType { get; set; }

        [JsonPropertyName("tokenFrom")]
        public string TokenFrom { get; private set; }

        [JsonPropertyName("tokenTo")]
        public string TokenTo { get; private set; }

        [JsonPropertyName("data")]
        public Dictionary<string, string> Data { get; set; }

        [JsonConstructor]
        public Message(string tokenFrom, string tokenTo) : this(tokenFrom, tokenTo, GeneralMessageType.User)
        {
        }

        public Message(string tokenFrom, string tokenTo, GeneralMessageType generalMessageType)
        {
            this.MessageType = generalMessageType;
            Data = new Dictionary<string, string>();
            this.TokenFrom = tokenFrom;
            this.TokenTo = tokenTo;
        }

        public Message<TUserCommand> GetReply()
        {
            return new Message<TUserCommand>(TokenTo, TokenFrom);
        }

        public string GetJson()
        {
            return JsonSerializer.Serialize(this, typeof(Message<TUserCommand>));
        }

        public static Message<TUserCommand> FromJson(string json)
        {
            return JsonSerializer.Deserialize<Message<TUserCommand>>(json);
        }

        public Message<TUserCommand> Add(string key, object value)
        {
            return Add(key, JsonSerializer.Serialize(value));
        }

        public Message<TUserCommand> Add(string key, string value)
        {
            if (Data.ContainsKey(key))
            {
                Data[key] = value;
            }
            else
            {
                Data.Add(key, value);
            }
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
