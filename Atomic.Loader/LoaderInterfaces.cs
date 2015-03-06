using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Atomic.Core;

namespace Atomic.Loader
{
    public interface IDataConverter
    {
        void Import(string sourceText);

        string Export();

        IProcessModel Model { get; }

        string FileExtension { get; }
    }

    public interface IElementModel<T> where T : IElement
    {
        string ID { get; set; }

        string Name { get; set; }

        void Import(ProcessModel model, T item);

        T Export(ExportRegistry reg);
    }

    public interface IProcessModel
    {
        string ID { get; set; }

        string Name { get; set; }

        EventModel[] Events { get; }

        TaskModel[] Tasks { get; }

        ConditionModel[] Conditions { get; }

        ValueModel[] Values { get; }

        FunctionModel[] Functions { get; }

        void Import(IProcess process);

        IProcess Export();
    }
}
