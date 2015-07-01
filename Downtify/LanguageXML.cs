using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace Downtify
{
    public class LanguageXML
    {

        private XmlDocument doc;

        public LanguageXML(string language)
        {
            doc = new XmlDocument();
            if(!File.Exists("language/" + language.ToLower() + ".xml"))
            {
                language = "en";
            }
            doc.Load("language/" + language.ToLower() + ".xml");
        }

        public string GetString(string key)
        {
            try
            {
                XmlNode node = doc.SelectSingleNode("lang/" + key);
                return node.InnerText;
            }
            catch (XPathException e)
            {
                return "Missing Language String: " + key;
            }
        }

    }
}
