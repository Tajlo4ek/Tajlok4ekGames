using System.Runtime.Serialization;

namespace DataStore
{
    [DataContract]
    public class Scenario
    {
        public enum ScenarioType
        {
            Text,
            Audio,
            Video,
            Image,
            Marker,
        }

        [DataMember]
        public ScenarioType Type { get; private set; }

        public bool IsMedia { get { return Type != ScenarioType.Text && Type != ScenarioType.Marker; } }

        [DataMember]
        public int Time { get; private set; }

        [DataMember]
        public string Data { get; private set; }

        public Scenario(string data, ScenarioType type, int time = -1)
        {
            Data = data;
            Type = type;
            Time = time;
        }

        public void Update(string workPath)
        {
            if (Type != ScenarioType.Text && Type != ScenarioType.Marker)
            {
                Data = workPath + @"\" + Data;
            }
        }

        public void UpdataData(string data)
        {
            Data = data;
        }

    }
}
