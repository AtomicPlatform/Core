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
        private string _name;
        private string _id;
        private EventModel[] _events;
        private TaskModel[] _tasks;
        private ConditionModel[] _conditions;

        public string Name
        {
            get { if (_name == null) _name = ""; return _name; }
            set { if (value == null) value = ""; _name = value; }
        }

        public string ID
        {
            get { if (_id == null) _id = ""; return _id; }
            set { if (value == null) value = ""; _id = value; }
        }

        public EventModel[] Events
        {
            get { if (_events == null) _events = new EventModel[0]; return _events; }
            set { if (value == null) value = new EventModel[0]; _events = value; }
        }

        public TaskModel[] Tasks
        {
            get { if (_tasks == null) _tasks = new TaskModel[0]; return _tasks; }
            set { if (value == null) value = new TaskModel[0]; _tasks = value; }
        }

        public ConditionModel[] Conditions
        {
            get { if (_conditions == null) _conditions = new ConditionModel[0]; return _conditions; }
            set { if (value == null) value = new ConditionModel[0]; _conditions = value; }
        }
    }

    public struct EventModel
    {
        private string _name;
        private string _id;
        private string _startConditionID;
        private string _stopConditionID;

        public string Name
        {
            get { if (_name == null) _name = ""; return _name; }
            set { if (value == null) value = ""; _name = value; }
        }

        public string ID
        {
            get { if (_id == null) _id = ""; return _id; }
            set { if (value == null) value = ""; _id = value; }
        }

        public string StartConditionID
        {
            get { if (_startConditionID == null) _startConditionID = ""; return _startConditionID; }
            set { if (value == null) value = ""; _startConditionID = value; }
        }

        public string StopConditionID
        {
            get { if (_stopConditionID == null) _stopConditionID = ""; return _stopConditionID; }
            set { if (value == null) value = ""; _stopConditionID = value; }
        }
    }


    public struct TaskModel
    {
        private string _name;
        private string _id;
        private string _startConditionID;
        private string _stopConditionID;
        private string _runScript;

        public string Name
        {
            get { if (_name == null) _name = ""; return _name; }
            set { if (value == null) value = ""; _name = value; }
        }

        public string ID
        {
            get { if (_id == null) _id = ""; return _id; }
            set { if (value == null) value = ""; _id = value; }
        }

        public string StartConditionID
        {
            get { if (_startConditionID == null) _startConditionID = ""; return _startConditionID; }
            set { if (value == null) value = ""; _startConditionID = value; }
        }

        public string StopConditionID
        {
            get { if (_stopConditionID == null) _stopConditionID = ""; return _stopConditionID; }
            set { if (value == null) value = ""; _stopConditionID = value; }
        }

        public string RunScript
        {
            get { if (_runScript == null) _runScript = ""; return _runScript; }
            set { if (value == null) value = ""; _runScript = value; }
        }
    }

    public struct ConditionModel
    {
        public enum TaskState
        {
            Done
        }

        private string _name;
        private string _id;
        private string _taskID;

        public string Name
        {
            get { if (_name == null) _name = ""; return _name; }
            set { if (value == null) value = ""; _name = value; }
        }

        public string ID
        {
            get { if (_id == null) _id = ""; return _id; }
            set { if (value == null) value = ""; _id = value; }
        }

        public string TaskID
        {
            get { if (_taskID == null) _taskID = ""; return _taskID; }
            set { if (value == null) value = ""; _taskID = value; }
        }

        public TaskState State { get; set; }
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
