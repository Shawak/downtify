using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Downtify
{
    public class XmlConfiguration
    {

        private string FileName;
        private Dictionary<string, string> Configuration = new Dictionary<string, string>();

        public XmlConfiguration(string file)
        {
            this.FileName = file;
        }

        public void SetConfigurationEntry(string entry, string value)
        {
            if(Configuration.ContainsKey(entry))
                Configuration.Remove(entry);
            Configuration.Add(entry, value);
        }

        public void LoadConfigurationFile()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(this.FileName);
            XmlNodeList nodes = doc.SelectSingleNode("configuration").ChildNodes;
            foreach(XmlNode node in nodes)
                SetConfigurationEntry(node.Name, node.InnerText);
        }

        public void SaveConfigurationFile()
        {
            XmlDocument doc = new XmlDocument();
            XmlNode root = doc.CreateElement("configuration");
            doc.AppendChild(root);
            foreach(string key in Configuration.Keys)
            {
                XmlNode xmlKey = doc.CreateElement(key);
                xmlKey.InnerText = Configuration[key];
                root.AppendChild(xmlKey);
            }
            if(File.Exists(this.FileName))
                File.Delete(this.FileName);
            doc.Save(this.FileName);
        }

        public string GetConfiguration(string entry)
        {
            return Configuration.ContainsKey(entry) ? Configuration[entry] : null;
        }

    }
}
