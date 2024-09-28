using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClientServer
{
    public class Message<TUserCommand>
    {
        public enum GeneralMessageType
        {
            User,

            Ping,

            GetReg,
            SendReg,

            GetFile,
            SendFile,

            FileNotExists,
            FileRecived,
            FileSended,

            Close
        }

        [JsonPropertyName("command")]
        public TUserCommand Command { get; set; }

        [JsonPropertyName("message")]
        public GeneralMessageType MessageType { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("data")]
        public Dictionary<string, string> Data { get; set; }

        public Message() : this(GeneralMessageType.User)
        {

        }

        public Message(GeneralMessageType generalMessageType)
        {
            this.MessageType = generalMessageType;
            Data = new Dictionary<string, string>();
            this.Token = "";
        }

        public string GetJson()
        {
            return JsonSerializer.Serialize(this, typeof(Message<TUserCommand>));
        }

        public Message<TUserCommand> SetToken(string token)
        {
            this.Token = token;
            return this;
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
            Data.Add(key, value);
            return this;
        }

        public Message<TUserCommand> SetCommand(TUserCommand command)
        {
            Command = command;
            return this;
        }

        public string GetData(string key)
        {
            if (Data.TryGetValue(key, out string value))
                return value;

            return "";
        }

        public void RemoveData(string key)
        {
            Data.Remove(key);
        }

    }
}
