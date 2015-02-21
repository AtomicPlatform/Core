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
        ProcessModel Import(string sourceText);

        string Export(ProcessModel sourceModel);
    }

    public struct ProcessModel
    {
        public string Name;
        
        public string ID;

        public EventModel[] Events;

        public TaskModel[] Tasks;

        public ConditionModel[] Conditions;
    }

    public struct EventModel
    {
        public string ID;

        public string Name;

        public string StartConditionID;

        public string StopConditionID;
    }


    public struct TaskModel
    {
        public string ID;

        public string Name;

        public string StartConditionID;

        public string StopConditionID;

        public string RunScript;
    }

    public struct ConditionModel
    {
        public enum TaskState
        {
            Done
        }

        public string ID;

        public string Name;

        public string TaskID;

        public TaskState State;
    }

    static public class DataLoader
    {
        static public IProcess Load(ProcessModel model)
        {
            IProcess p = new AtomicProcess();

            return p;
        }

        static public ProcessModel Save(IProcess p)
        {
            ProcessModel model = new ProcessModel();

            return model;
        }
    }
}
