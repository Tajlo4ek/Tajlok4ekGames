using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace DataStore
{
    [DataContract]
    public class PackInfo
    {
        [DataMember]
        public readonly string Authors;

        public PackInfo(string data)
        {
            Authors = data;
        }

        public PackInfo(XmlElement item)
        {
            var data = Utils.MyXmlUtils.GetNodeWithName(item, "authors");

            StringBuilder sb = new StringBuilder();

            if (data.Count != 0)
            {
                foreach (var author in Utils.MyXmlUtils.GetNodeWithName(data[0], "author"))
                {
                    sb.AppendLine(author.InnerText);
                }
            }

            Authors = sb.ToString();
        }


    }
}
