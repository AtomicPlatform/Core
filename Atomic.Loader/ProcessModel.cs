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
    public class RefIdModel
    {
        static public string GetModelID(object o)
        {
            return o.GetType().Name + o.GetHashCode().ToString();
        }

        private string _id;

        [XmlAttribute(AttributeName = "id")]
        public string ID
        {
            get { if (_id == null) _id = ""; return _id; }
            set { if (value == null) value = ""; _id = value; }
        }
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

        void Import(IProcess process);

        IProcess Export();
    }

    abstract public class ElementModel<T> : IElementModel<T> where T : IElement
    {
        [XmlAttribute(AttributeName = "id")]
        public string ID { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        virtual public void Import(ProcessModel model, T item)
        {
            ID = RefIdModel.GetModelID(item);
            Name = item.Name;
        }

        virtual public T Export(ExportRegistry reg)
        {
            T item = GetElement(reg);
            item.Name = Name;

            return item;
        }

        abstract protected T GetElement(ExportRegistry reg);

        protected RefIdModel CreateConditionReference(ProcessModel model, ICondition cond)
        {
            ConditionModel condModel = new ConditionModel();
            condModel.Import(model, cond);
            model.AddCondition(condModel);

            RefIdModel idModel = new RefIdModel();
            idModel.ID = RefIdModel.GetModelID(cond);

            return idModel;
        }

        protected RefIdModel CreateTaskReference(ProcessModel model, IRunnable task)
        {
            if (task is IProcess)
            {
                return new RefIdModel() { ID = model.ID };
            }

            TaskModel taskModel = new TaskModel();
            taskModel.Import(model, task);
            model.AddTask(taskModel);

            RefIdModel idModel = new RefIdModel();
            idModel.ID = RefIdModel.GetModelID(task);

            return idModel;
        }
    }

    public class ProcessModel : IProcessModel
    {
        private IDictionary<string, EventModel> _events = new Dictionary<string, EventModel>();
        private IDictionary<string, TaskModel> _tasks = new Dictionary<string, TaskModel>();
        private IDictionary<string, ConditionModel> _conditions = new Dictionary<string, ConditionModel>();
        private IDictionary<string, ValueModel> _values = new Dictionary<string, ValueModel>();

        [XmlAttribute(AttributeName = "id")]
        public string ID { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        internal void AddEvent(EventModel evtModel)
        {
            if (_events.ContainsKey(evtModel.ID)) return;
            _events[evtModel.ID] = evtModel;
        }

        internal void AddTask(TaskModel taskModel)
        {
            if (_tasks.ContainsKey(taskModel.ID)) return;
            _tasks[taskModel.ID] = taskModel;
        }

        internal void AddCondition(ConditionModel condModel)
        {
            if (_conditions.ContainsKey(condModel.ID)) return;
            _conditions[condModel.ID] = condModel;
        }

        internal void AddValue(ValueModel valModel)
        {
            if (_values.ContainsKey(valModel.ID)) return;
            _values[valModel.ID] = valModel;
        }

        public EventModel[] Events
        {
            get { return _events.Values.ToArray(); }
        }

        public TaskModel[] Tasks
        {
            get { return _tasks.Values.ToArray(); }
        }

        public ConditionModel[] Conditions
        {
            get { return _conditions.Values.ToArray(); }
        }

        public ValueModel[] Values
        {
            get { return _values.Values.ToArray(); }
        }

        virtual public void Import(IProcess process)
        {
            ID = RefIdModel.GetModelID(process);
            Name = process.Name;

            EventModel evtModel = new EventModel();
            evtModel.Import(this, process.StartEvent);
            AddEvent(evtModel);

            evtModel = new EventModel();
            evtModel.Import(this, process.StopEvent);
            AddEvent(evtModel);

            foreach (IEvent evt in process.Events)
            {
                evtModel = new EventModel();
                evtModel.Import(this, evt);
                AddEvent(evtModel);
            }

            foreach (ITask task in process.Tasks)
            {
                TaskModel taskModel = new TaskModel();
                taskModel.Import(this, task);
                AddTask(taskModel);
            }

            ConditionModel condModel = new ConditionModel();
            condModel.Import(this, process.StartCondition);
            AddCondition(condModel);

            condModel = new ConditionModel();
            condModel.Import(this, process.StopCondition);
            AddCondition(condModel);

            foreach (IValue v in process.Values)
            {
                ValueModel valModel = new ValueModel();
                valModel.Import(this, v);
                AddValue(valModel);
            }

            // remove default references

            // remove unreferenced items
        }

        public IProcess Export()
        {
            ExportRegistry reg = new ExportRegistry(this);

            IProcess p = new AtomicProcess();
            p.Name = Name;

            return p;
        }
    }

    public struct EventListModel
    {
        private EventModel[] _events;

        [XmlElement(ElementName = "event")]
        public EventModel[] Event
        {
            get { if (_events == null) _events = new EventModel[0]; return _events; }
            set { if (value == null) value = new EventModel[0]; _events = value; }
        }
    }

    [XmlRoot(ElementName = "event")]
    public class EventModel : ElementModel<IEvent>
    {
        private RefIdModel _conditionID;

        [XmlElement(ElementName = "startConditionID")]
        public RefIdModel Condition
        {
            get { return _conditionID; }
            set { _conditionID = value; }
        }

        override public void Import(ProcessModel model, IEvent evt)
        {
            base.Import(model, evt);
            Condition = CreateConditionReference(model, evt.Condition);
        }

        override public IEvent Export(ExportRegistry reg)
        {
            IEvent evt = base.Export(reg);
            evt.Condition = reg.GetCondition(Condition.ID);

            return evt;
        }

        protected override IEvent GetElement(ExportRegistry reg)
        {
            return reg.GetEvent(ID);
        }
    }

    public struct TaskListModel
    {
        private TaskModel[] _tasks;

        [XmlElement(ElementName = "task")]
        public TaskModel[] Task
        {
            get { if (_tasks == null) _tasks = new TaskModel[0]; return _tasks; }
            set { if (value == null) value = new TaskModel[0]; _tasks = value; }
        }
    }

    public class TaskModel : ElementModel<IRunnable>
    {
        public enum TaskState
        {
            Ready,
            Active,
            Running,
            RunComplete,
            Stopping,
            Done
        }

        private RefIdModel _startConditionID;
        private RefIdModel _stopConditionID;
        private string _runScript;

        [XmlElement(ElementName = "startCondition")]
        public RefIdModel StartCondition
        {
            get { return _startConditionID; }
            set { _startConditionID = value; }
        }

        [XmlElement(ElementName = "stopCondition")]
        public RefIdModel StopCondition
        {
            get { return _stopConditionID; }
            set { _stopConditionID = value; }
        }

        [XmlElement(ElementName = "runScript")]
        public string RunScript
        {
            get { if (_runScript == null) _runScript = ""; return _runScript; }
            set { if (value == null) value = ""; _runScript = value; }
        }

        override public void Import(ProcessModel model, IRunnable task)
        {
            base.Import(model, task);
            if (task is ITask)
            {
                RunScript = ((ITask)task).FunctionText;
            }

            StartCondition = CreateConditionReference(model, task.StartCondition);
            StopCondition = CreateConditionReference(model, task.StopCondition);
        }

        override public IRunnable Export(ExportRegistry reg)
        {
            IRunnable t = base.Export(reg);

            return t;
        }

        protected override IRunnable GetElement(ExportRegistry reg)
        {
            return reg.GetTask(ID);
        }

        static internal TaskState GetState(Atomic.Core.TaskState taskState)
        {
            TaskState modelState;

            Enum.TryParse<TaskState>(Enum.GetName(typeof(Atomic.Core.TaskState), taskState), true, out modelState);

            return modelState;
        }
    }


    public struct ConditionListModel
    {
        private ConditionModel[] _conditions;

        [XmlElement(ElementName = "condition")]
        public ConditionModel[] Condition
        {
            get { if (_conditions == null) _conditions = new ConditionModel[0]; return _conditions; }
            set { if (value == null) value = new ConditionModel[0]; _conditions = value; }
        }
    }

    public class ConditionModel : ElementModel<ICondition>
    {
        private RefIdModel _taskID;

        [XmlElement(ElementName = "task")]
        public RefIdModel Task
        {
            get { return _taskID; }
            set { _taskID = value; }
        }

        public Atomic.Loader.TaskModel.TaskState State { get; set; }

        override public void Import(ProcessModel model, ICondition cond)
        {
            base.Import(model, cond);

            if (cond is TaskCondition)
            {
                TaskCondition taskCond = (TaskCondition)cond;
                Task = CreateTaskReference(model, taskCond.Task);
                State = TaskModel.GetState(taskCond.State);
            }
        }

        override public ICondition Export(ExportRegistry reg)
        {
            ICondition cond = base.Export(reg);

            return cond;
        }

        protected override ICondition GetElement(ExportRegistry reg)
        {
            return reg.GetCondition(ID);
        }
    }

    public class ValueModel : ElementModel<IValue>
    {
        [XmlText()]
        public object Value { get; set; }

        override public void Import(ProcessModel model, IValue v)
        {
            base.Import(model, v);
            Value = v.Value;
        }

        override public IValue Export(ExportRegistry reg)
        {
            IValue v = base.Export(reg);

            return v;
        }

        protected override IValue GetElement(ExportRegistry reg)
        {
            return reg.GetValue(ID);
        }
    }

    public class JsonProcessModel : ProcessModel
    {
        new public EventModel[] Events
        {
            get { return base.Events.ToArray(); }
            set
            {
                foreach (EventModel evtModel in value)
                {
                    base.AddEvent(evtModel);
                }
            }
        }

        new public TaskModel[] Tasks
        {
            get { return base.Tasks.ToArray(); }
            set
            {
                foreach (TaskModel taskModel in value)
                {
                    base.AddTask(taskModel);
                }
            }
        }

        new public ConditionModel[] Conditions
        {
            get { return base.Conditions.ToArray(); }
            set
            {
                foreach (ConditionModel condModel in value)
                {
                    base.AddCondition(condModel);
                }
            }
        }
    }

    [XmlRoot(Namespace = "http://www.atomicplatform.com/Process",
     ElementName = "process")]
    public class XmlProcessModel : ProcessModel
    {
        private EventListModel _events;
        private TaskListModel _tasks;
        private ConditionListModel _conditions;

        [XmlElement(ElementName = "events")]
        new public EventListModel Events
        {
            get { return _events; }
            set { _events = value; }
        }

        [XmlElement(ElementName = "tasks")]
        new public TaskListModel Tasks
        {
            get { return _tasks; }
            set { _tasks = value; }
        }

        [XmlElement(ElementName = "conditions")]
        new public ConditionListModel Conditions
        {
            get { return _conditions; }
            set { _conditions = value; }
        }

        public override void Import(IProcess process)
        {
            base.Import(process);

            EventListModel evtListModel = Events;
            evtListModel.Event = base.Events;
            Events = evtListModel;

            TaskListModel taskListModel = Tasks;
            taskListModel.Task = base.Tasks;
            Tasks = taskListModel;

            ConditionListModel condListModel = Conditions;
            condListModel.Condition = base.Conditions;
            Conditions = condListModel;
        }
    }

}
