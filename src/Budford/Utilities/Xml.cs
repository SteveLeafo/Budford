using System.Xml;
using System.Xml.Linq;

namespace Budford.Utilities
{
    internal static class Xml
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gd"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetValue(XElement gd, string name)
        {
            if (gd != null)
            {
                var element = gd.Element(name);
                if (element != null)
                {
                    return element.Value;
                }
            }
            return "";
        }

        public static string XmlEscape(string unescaped)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode node = doc.CreateElement("root");
            node.InnerText = unescaped;
            return node.InnerXml;
        }

        public static string XmlUnescape(string escaped)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode node = doc.CreateElement("root");
            node.InnerXml = escaped;
            return node.InnerText;
        }
    }    
}
