using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atomic.Loader
{
    public class BpmnConverter : IDataConverter
    {
        private IProcessModel _model = null;

        public void Import(string sourceText)
        {
            throw new NotImplementedException();
        }

        public string Export()
        {
            throw new NotImplementedException();
        }

        public IProcessModel Model
        {
            get { return _model; }
            private set { _model = (IProcessModel)value; }
        }

    }
}
