using System.Text.Json.Serialization;

namespace ClientServer
{
    internal class FilePart
    {
        [JsonIgnore]
        public int MaxSize { get; private set; }

        public FilePart(int maxSize)
        {
            MaxSize = maxSize;
            Data = new byte[maxSize];
        }

        [JsonPropertyName("data")]
        public byte[] Data { get; set; }

        [JsonPropertyName("available")]
        public int Available { get; set; }
    }
}
