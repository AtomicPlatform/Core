using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atomic.Loader
{
    public class JsonConverter : IDataConverter
    {
        public ProcessModel Import(string sourceText)
        {
            return (ProcessModel)Newtonsoft.Json.JsonConvert.DeserializeObject(sourceText, typeof(ProcessModel));
        }

        public string Export(ProcessModel sourceModel)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(sourceModel);
        }
    }
}
