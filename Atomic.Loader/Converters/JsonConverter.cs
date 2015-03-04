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
        private ProcessModel _model = new ProcessModel();

        public string FileExtension { get { return "json"; } }

        public void Import(string sourceText)
        {
            Model = (ProcessModel)JsonConvert.DeserializeObject(
                sourceText, 
                typeof(ProcessModel)
            );
        }

        public string Export()
        {
            return JsonConvert.SerializeObject((ProcessModel)Model);
        }

        public IProcessModel Model
        {
            get { return _model; }
            private set { _model = (ProcessModel)value; }
        }

    }
}
