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

        private String file;
        private Dictionary<String, String> configuration = new Dictionary<String, String>();

        public XmlConfiguration(String file)
        {
            this.file = file;
        }

        public void SetConfigurationEntry(String entry, String value)
        {
            if(configuration.ContainsKey(entry))
            {
                configuration.Remove(entry);
            }
            configuration.Add(entry, value);
        }

        public void LoadConfigurationFile()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(this.file);
            XmlNodeList nodes = doc.SelectSingleNode("configuration").ChildNodes;
            foreach(XmlNode node in nodes)
            {
                SetConfigurationEntry(node.Name, node.InnerText);
            }
        }

        public void SaveConfigurationFile()
        {
            XmlDocument doc = new XmlDocument();
            XmlNode root = doc.CreateElement("configuration");
            doc.AppendChild(root);
            foreach(String key in configuration.Keys)
            {
                XmlNode xmlKey = doc.CreateElement(key);
                xmlKey.InnerText = configuration[key];
                root.AppendChild(xmlKey);
            }
            if(File.Exists(this.file))
            {
                File.Delete(this.file);
            }
            doc.Save(this.file);
        }

        public String GetConfiguration(String entry)
        {
            if(configuration.ContainsKey(entry))
            {
                return configuration[entry];
            }
            return null;
        }

    }
}
