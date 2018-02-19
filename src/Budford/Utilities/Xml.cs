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
    }    
}
