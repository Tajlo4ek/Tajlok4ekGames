using System.Collections.Generic;
using System.Xml;

namespace DataStore.Utils
{
    static class MyXmlUtils
    {
        public static List<XmlElement> GetNodeWithName(XmlElement item, string name)
        {
            List<XmlElement> elements = new List<XmlElement>();

            foreach (var child in item.ChildNodes)
            {
                if (child is XmlElement node)
                {
                    if (node.LocalName.Equals(name))
                        elements.Add(node);
                }
            }

            return elements;
        }
    }
}
