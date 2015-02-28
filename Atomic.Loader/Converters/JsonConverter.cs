using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Atomic.Loader
{
    public class JsonConverter : IDataConverter
    {
        private JsonProcessModel _model = new JsonProcessModel();

        public void Import(string sourceText)
        {
            Model = (JsonProcessModel)JsonConvert.DeserializeObject(
                sourceText, 
                typeof(JsonProcessModel)
            );
        }

        public string Export()
        {
            return JsonConvert.SerializeObject((JsonProcessModel)Model);
        }

        public IProcessModel Model
        {
            get { return _model; }
            private set { _model = (JsonProcessModel)value; }
        }

    }
}
