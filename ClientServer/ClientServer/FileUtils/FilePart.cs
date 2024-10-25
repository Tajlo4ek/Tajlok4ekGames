using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
