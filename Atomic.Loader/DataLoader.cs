using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml.Schema;

using Atomic.Core;

namespace Atomic.Loader
{
    public interface IDataConverter
    {
        void Import(string sourceText);

        string Export();

        IProcessModel Model { get; }
    }

    static public class DataLoader
    {
        static public IProcess Load(IProcessModel model)
        {
            IProcess p = new AtomicProcess();

            return p;
        }

        static public IProcessModel Save(Type modelType, IProcess p)
        {
            IProcessModel model = (IProcessModel)Activator.CreateInstance(modelType);

            return model;
        }
    }
}
