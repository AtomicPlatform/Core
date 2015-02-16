using System;
using System.Collections.Generic;
using System.Text;

namespace Atomic.Loader
{
    public class XmlEntryConverter : IEntryConverter<string>
    {
        public XmlEntryConverter()
        {
        }

        public string Export(IDictionary<string, object> attributes)
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            buffer.Append("<atomic:app xmlns:atomic=\"http://www.atomicplatform.com/Process\">");

            foreach (string key in attributes.Keys)
            {
                buffer.Append("<" + key + ">");
                switch (key)
                {
                    case "process":
                        buffer.Append(ExportObject((IDictionary<string, string>)attributes[key]));
                        break;
                    default:
                        buffer.Append(ExportObjectList((List<IDictionary<string, string>>)attributes[key]));
                        break;
                }
                buffer.Append("</" + key + ">");
            }
            buffer.Append("</atomic:app>");

            return buffer.ToString();
        }

        private string ExportObject(IDictionary<string, string> attributes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> keyPair in attributes)
            {
                switch (keyPair.Key)
                {
                    case "category":
                    case "id":
                        break;
                    default:
                        if (keyPair.Value.Length == 0) continue;

                        sb.Append("<" + keyPair.Key + ">");
                        sb.Append(keyPair.Value);
                        sb.Append("</" + keyPair.Key + ">");
                        break;
                }
            }

            return sb.ToString();
        }

        private string ExportObjectList(List<IDictionary<string, string>> attributeList)
        {
            StringBuilder sb = new StringBuilder();
            string elementName = "";
            foreach (IDictionary<string, string> attributes in attributeList)
            {
                elementName = attributes["category"];
                sb.Append("<" + elementName + " id=\"" + attributes["id"] + "\">");
                sb.Append(ExportObject(attributes));
                sb.Append("</" + elementName + ">");
            }

            return sb.ToString();
        }


        public IDictionary<string, object> Import(string source)
        {
            throw new NotImplementedException();
        }
    }
}
