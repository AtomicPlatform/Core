using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Atomic.Loader
{
    public class JsonEntryConverter : IEntryConverter<string>
    {
        public string Export(IDictionary<string, object> attributes)
        {
            return JsonConvert.SerializeObject(attributes);
        }

        public IDictionary<string, object> Import(string jsonString)
        {
            IDictionary<string, object> attributes = new Dictionary<string, object>();

            JObject obj = (JObject)JsonConvert.DeserializeObject(jsonString);
            object item = null;
            foreach (JProperty property in obj.Properties())
            {
                switch (property.Name)
                {
                    case "process":
                        item = ImportObject((JObject)property.Value);
                        break;
                    default:
                        item = ImportObjectList((JArray)property.Value);
                        break;
                }

                attributes[property.Name] = item;
            }

            return attributes;
        }

        private IDictionary<string, string> ImportObject(JObject o)
        {
            IDictionary<string, string> item = new Dictionary<string, string>();
            foreach (JProperty k in o.Properties())
            {
                item[k.Name] = k.Value.ToString();
            }

            return item;
        }

        private List<IDictionary<string, string>> ImportObjectList(JArray listObject)
        {
            List<IDictionary<string, string>> itemList = new List<IDictionary<string, string>>();

            foreach (JObject prop in listObject)
            {
                itemList.Add(ImportObject(prop));
            }

            return itemList;
        }
    }
}
