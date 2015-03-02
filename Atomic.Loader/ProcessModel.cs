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

        EventModel StartEvent { get; set; }

        EventModel StopEvent { get; set; }

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
            ConditionModel condModel = model.AddCondition(cond);

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

            TaskModel taskModel = model.AddTask((ITask)task);

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

        [XmlElement(ElementName = "startOnEvent")]
        public EventModel StartEvent { get; set; }

        [XmlElement(ElementName = "stopOnEvent")]
        public EventModel StopEvent { get; set; }

        internal EventModel AddEvent(IEvent evt)
        {
            EventModel evtModel = CreateEventModel();
            evtModel.Import(this, evt);

            _events[evtModel.ID] = evtModel;
            return evtModel;
        }

        internal TaskModel AddTask(ITask task)
        {
            TaskModel taskModel = CreateTaskModel();
            taskModel.Import(this, task);

            _tasks[taskModel.ID] = taskModel;
            return taskModel;
        }

        internal ConditionModel AddCondition(ICondition cond)
        {
            ConditionModel condModel = CreateConditionModel();
            condModel.Import(this, cond);

            _conditions[condModel.ID] = condModel;
            return condModel;
        }

        internal ValueModel AddValue(IValue val)
        {
            ValueModel valModel = CreateValueModel();
            valModel.Import(this, val);

            _values[valModel.ID] = valModel;
            return valModel;
        }

        virtual public EventModel[] Events
        {
            get { return _events.Values.ToArray(); }
            set
            {
                _events.Clear();
                foreach (EventModel model in value)
                {
                    _events[model.ID] = model;
                }
            }
        }

        virtual public TaskModel[] Tasks
        {
            get { return _tasks.Values.ToArray(); }
            set
            {
                _tasks.Clear();
                foreach (TaskModel model in value)
                {
                    _tasks[model.ID] = model;
                }
            }
        }

        virtual public ConditionModel[] Conditions
        {
            get { return _conditions.Values.ToArray(); }
            set
            {
                _conditions.Clear();
                foreach (ConditionModel model in value)
                {
                    _conditions[model.ID] = model;
                }
            }
        }

        virtual public ValueModel[] Values
        {
            get { return _values.Values.ToArray(); }
            set
            {
                _values.Clear();
                foreach (ValueModel model in value)
                {
                    _values[model.ID] = model;
                }
            }
        }

        virtual public void Import(IProcess process)
        {
            ID = RefIdModel.GetModelID(process);
            Name = process.Name;

            StartEvent = CreateEventModel();
            StartEvent.Import(this, process.StartEvent);

            StopEvent = CreateEventModel();
            StopEvent.Import(this, process.StopEvent);

            foreach (IEvent evt in process.Events)
            {
                AddEvent(evt);
            }

            foreach (ITask task in process.Tasks)
            {
                AddTask(task);
            }

            foreach (IValue v in process.Values)
            {
                AddValue(v);
            }

            // remove default references

            // remove unreferenced items
        }

        virtual protected EventModel CreateEventModel()
        {
            return new EventModel();
        }

        virtual protected TaskModel CreateTaskModel()
        {
            return new TaskModel();
        }

        virtual protected ConditionModel CreateConditionModel()
        {
            return new ConditionModel();
        }

        virtual protected ValueModel CreateValueModel()
        {
            return new ValueModel();
        }

        public IProcess Export()
        {
            ExportRegistry reg = new ExportRegistry(this);
            IProcess p = reg.Process;
            p.Name = Name;

            ExportEvent(reg, p.StartEvent, StartEvent);
            ExportEvent(reg, p.StopEvent, StopEvent);

            foreach (ConditionModel condModel in Conditions)
            {
                ExportCondition(reg, reg.GetCondition(condModel.ID), condModel);
            }

            return p;
        }

        private void ExportCondition(ExportRegistry reg, ICondition cond, ConditionModel condModel)
        {
            cond.Name = condModel.Name;
            if (cond is TaskCondition)
            {
                TaskCondition taskCond = (TaskCondition)cond;
                taskCond.Task = reg.GetTask(condModel.Task.ID);
                taskCond.State = (Atomic.Core.TaskState)Enum.Parse(typeof(Atomic.Core.TaskState), condModel.State.ToString());
            }
        }

        private void ExportEvent(ExportRegistry reg, IEvent evt, EventModel evtModel)
        {
            evt.Name = StartEvent.Name;
            evt.StartCondition = reg.GetCondition(evtModel.StartCondition.ID);
            evt.StopCondition = reg.GetCondition(evtModel.StopCondition.ID);
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
        private RefIdModel _startConditionID;
        private RefIdModel _stopConditionID;

        [XmlElement(ElementName = "startOnCondition")]
        public RefIdModel StartCondition
        {
            get { return _startConditionID; }
            set { _startConditionID = value; }
        }

        [XmlElement(ElementName = "stopOnCondition")]
        public RefIdModel StopCondition
        {
            get { return _stopConditionID; }
            set { _stopConditionID = value; }
        }

        override public void Import(ProcessModel model, IEvent evt)
        {
            base.Import(model, evt);
            StartCondition = CreateConditionReference(model, evt.StartCondition);
            StopCondition = CreateConditionReference(model, evt.StopCondition);
        }

        override public IEvent Export(ExportRegistry reg)
        {
            IEvent evt = base.Export(reg);
            evt.StartCondition = reg.GetCondition(StartCondition.ID);
            evt.StopCondition = reg.GetCondition(StopCondition.ID);

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

    public class TaskModel : ElementModel<ITask>
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

        [XmlElement(ElementName = "startOnCondition")]
        public RefIdModel StartCondition
        {
            get { return _startConditionID; }
            set { _startConditionID = value; }
        }

        [XmlElement(ElementName = "stopOnCondition")]
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

        override public void Import(ProcessModel model, ITask task)
        {
            base.Import(model, task);
            if (task is ITask)
            {
                RunScript = ((ITask)task).FunctionText;
            }

            StartCondition = CreateConditionReference(model, task.StartCondition);
            StopCondition = CreateConditionReference(model, task.StopCondition);
        }

        override public ITask Export(ExportRegistry reg)
        {
            ITask t = base.Export(reg);

            return t;
        }

        protected override ITask GetElement(ExportRegistry reg)
        {
            return (ITask)reg.GetTask(ID);
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

        [XmlElement(ElementName = "state")]
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

    public struct ValueListModel
    {
        private ValueModel[] _values;

        [XmlElement(ElementName = "value")]
        public ValueModel[] Value
        {
            get { if (_values == null) _values = new ValueModel[0]; return _values; }
            set { if (value == null) value = new ValueModel[0]; _values = value; }
        }
    }

    public class ValueModel : ElementModel<IValue>
    {
        [XmlText()]
        public string Value { get; set; }

        override public void Import(ProcessModel model, IValue v)
        {
            base.Import(model, v);
            Value = v.Value.ToString();
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


    [XmlRoot(Namespace = "http://www.atomicplatform.com/Process",
     ElementName = "process")]
    public class XmlProcessModel : ProcessModel
    {
        private EventListModel _events;
        private TaskListModel _tasks;
        private ConditionListModel _conditions;
        private ValueListModel _values;

        [XmlElement(ElementName = "events")]
        public EventListModel EventList
        {
            get { return _events; }
            set { _events = value; }
        }

        [XmlElement(ElementName = "tasks")]
        public TaskListModel TaskList
        {
            get { return _tasks; }
            set { _tasks = value; }
        }

        [XmlElement(ElementName = "conditions")]
        public ConditionListModel ConditionList
        {
            get { return _conditions; }
            set { _conditions = value; }
        }

        [XmlElement(ElementName = "values")]
        public ValueListModel ValueList
        {
            get { return _values; }
            set { _values = value; }
        }

        override public ConditionModel[] Conditions
        {
            get { return _conditions.Condition; }
            set {}
        }

        override public EventModel[] Events
        {
            get { return _events.Event; }
            set { }
        }

        override public TaskModel[] Tasks
        {
            get { return _tasks.Task; }
            set { }
        }

        override public ValueModel[] Values
        {
            get { return _values.Value; }
            set { }
        }

        public override void Import(IProcess process)
        {
            base.Import(process);

            EventListModel evtListModel = EventList;
            evtListModel.Event = base.Events;
            EventList = evtListModel;

            TaskListModel taskListModel = TaskList;
            taskListModel.Task = base.Tasks;
            TaskList = taskListModel;

            ConditionListModel condListModel = ConditionList;
            condListModel.Condition = base.Conditions;
            ConditionList = condListModel;
        }
    }

}
