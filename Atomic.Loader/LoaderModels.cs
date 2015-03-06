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
        private string _id;

        [XmlAttribute(AttributeName = "id")]
        public string ID
        {
            get { if (_id == null) _id = ""; return _id; }
            set { if (value == null) value = ""; _id = value; }
        }
    }



    abstract public class ElementModel<T> : IElementModel<T> where T : IElement
    {
        [XmlAttribute(AttributeName = "id")]
        public string ID { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        virtual public void Import(ProcessModel model, T item)
        {
            ID = item.ID;
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
            idModel.ID = condModel.ID;

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
            idModel.ID = taskModel.ID;

            return idModel;
        }

        protected RefIdModel CreateValueReference(ProcessModel model, IValue value)
        {
            ValueModel valModel = model.AddValue(value);
            RefIdModel idModel = new RefIdModel();
            idModel.ID = valModel.ID;

            return idModel;
        }

        protected RefIdModel CreateFunctionReference(ProcessModel model, IFunction func)
        {
            FunctionModel funcModel = model.AddFunction(func);
            RefIdModel idModel = new RefIdModel();
            idModel.ID = funcModel.ID;

            return idModel;
        }
    }

    [XmlRoot(Namespace = "http://www.atomicplatform.com/Process", ElementName = "process")]
    public class ProcessModel : IProcessModel
    {
        private IDictionary<string, EventModel> _events = new Dictionary<string, EventModel>();
        private IDictionary<string, TaskModel> _tasks = new Dictionary<string, TaskModel>();
        private IDictionary<string, ConditionModel> _conditions = new Dictionary<string, ConditionModel>();
        private IDictionary<string, ValueModel> _values = new Dictionary<string, ValueModel>();
        private IDictionary<string, FunctionModel> _functions = new Dictionary<string, FunctionModel>();

        [XmlAttribute(AttributeName = "id")]
        public string ID { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        internal EventModel AddEvent(IEvent evt)
        {
            EventModel evtModel = null;
            if (_events.ContainsKey(evt.ID))
            {
                evtModel = _events[evt.ID];
            }
            else
            {
                evtModel = CreateEventModel(evt);
            }

            return evtModel;
        }

        internal TaskModel AddTask(ITask task)
        {
            TaskModel taskModel = null;
            if (_tasks.ContainsKey(task.ID))
            {
                taskModel = _tasks[task.ID];
            }
            else
            {
                taskModel = CreateTaskModel(task);
            }

            return taskModel;
        }

        internal ConditionModel AddCondition(ICondition cond)
        {
            ConditionModel condModel = null;
            if (_conditions.ContainsKey(cond.ID))
            {
                condModel = _conditions[cond.ID];
            }
            else
            {
                condModel = CreateConditionModel(cond);
            }

            return condModel;
        }

        internal ValueModel AddValue(IValue val)
        {
            ValueModel valModel = null;
            if (_conditions.ContainsKey(val.ID))
            {
                valModel = _values[val.ID];
            }
            else
            {
                valModel = CreateValueModel(val);
            }

            return valModel;
        }

        internal FunctionModel AddFunction(IFunction func)
        {
            FunctionModel funcModel = null;
            if (_functions.ContainsKey(func.ID))
            {
                funcModel = _functions[func.ID];
            }
            else
            {
                FunctionModel existModel = _functions.Values.Where(
                    x => x.AssemblyName == func.AsmName 
                        && x.MethodName == func.MethodName 
                        && x.ModuleName == func.ModuleName).FirstOrDefault();

                funcModel = (existModel == null) ? CreateFunctionModel(func) : existModel;
            }

            return funcModel;
        }

        [XmlArray(ElementName = "events")]
        [XmlArrayItem(ElementName="event")]
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

        [XmlArray(ElementName = "tasks")]
        [XmlArrayItem(ElementName = "task")]
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

        [XmlArray(ElementName = "conditions")]
        [XmlArrayItem(ElementName = "condition")]
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

        [XmlArray(ElementName = "values")]
        [XmlArrayItem(ElementName = "value")]
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

        [XmlArray(ElementName = "functions")]
        [XmlArrayItem(ElementName = "function")]
        virtual public FunctionModel[] Functions
        {
            get { return _functions.Values.ToArray(); }
            set
            {
                _functions.Clear();
                foreach (FunctionModel model in value)
                {
                    _functions[model.ID] = model;
                }
            }
        }

        virtual public void Import(IProcess process)
        {
            ID = process.ID;
            Name = process.Name;

            AddCondition(process.DoneCondition);

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

        virtual protected EventModel CreateEventModel(IEvent evt)
        {
            EventModel evtModel = new EventModel() { ID = evt.ID, Name = evt.Name };
            _events[evt.ID] = evtModel;
            evtModel.Import(this, evt);

            return evtModel;
        }

        virtual protected FunctionModel CreateFunctionModel(IFunction func)
        {
            FunctionModel funcModel = new FunctionModel() { ID = func.ID, Name = func.Name };
            _functions[func.ID] = funcModel;
            funcModel.Import(this, func);

            return funcModel;
        }

        virtual protected TaskModel CreateTaskModel(ITask task)
        {
            TaskModel taskModel = new TaskModel() { ID = task.ID, Name = task.Name };
            _tasks[task.ID] = taskModel;
            taskModel.Import(this, task);

            return taskModel;
        }

        virtual protected ConditionModel CreateConditionModel(ICondition cond)
        {
            ConditionModel condModel = new ConditionModel() { ID = cond.ID, Name = cond.Name };
            _conditions[cond.ID] = condModel;
            condModel.Import(this, cond);

            return condModel;
        }

        virtual protected ValueModel CreateValueModel(IValue val)
        {
            ValueModel valModel = new ValueModel() { ID = val.ID, Name = val.Name };
            _values[val.ID] = valModel;
            valModel.Import(this, val);

            return valModel;
        }

        public IProcess Export()
        {
            ExportRegistry reg = new ExportRegistry(this);
            IProcess p = reg.Process;
            p.Name = Name;

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
            evt.Name = evtModel.Name;
            //evt.StartCondition = reg.GetCondition(evtModel.StartCondition.ID);
        }
    }

    public class EventModel : ElementModel<IEvent>
    {
        public const string StartEventType = "Start";
        public const string StopEventType = "Stop";

        private RefIdModel _startConditionID;
        private string _type;

        [XmlElement(ElementName = "startOnCondition")]
        public RefIdModel StartCondition
        {
            get { return _startConditionID; }
            set { _startConditionID = value; }
        }

        [XmlElement(ElementName = "eventType")]
        public string EventType
        {
            get { return _type; }
            set { _type = value; }
        }

        override public void Import(ProcessModel model, IEvent evt)
        {
            base.Import(model, evt);
            StartCondition = CreateConditionReference(model, evt.StartCondition);
            if (evt is StartEvent)
            {
                EventType = StartEventType;
            }
            else if (evt is StopEvent)
            {
                EventType = StopEventType;
            }
        }

        override public IEvent Export(ExportRegistry reg)
        {
            IEvent evt = base.Export(reg);
            //evt.StartCondition = reg.GetCondition(StartCondition.ID);

            return evt;
        }

        protected override IEvent GetElement(ExportRegistry reg)
        {
            return reg.GetEvent(ID);
        }
    }

    [XmlRoot(ElementName="function")]
    public class FunctionModel : ElementModel<IFunction>
    {
        [XmlElement(ElementName = "assembly", Namespace="")]
        public string AssemblyName { get; set; }

        [XmlElement(ElementName = "module", Namespace="")]
        public string ModuleName { get; set; }

        [XmlElement(ElementName = "method", Namespace="")]
        public string MethodName { get; set; }

        public override void Import(ProcessModel model, IFunction item)
        {
            base.Import(model, item);
            
            AssemblyName = item.AsmName;
            ModuleName = item.ModuleName;
            MethodName = item.MethodName;
        }

        public override IFunction Export(ExportRegistry reg)
        {
            IFunction func = base.Export(reg);
            func.SetProperties(AssemblyName, MethodName, ModuleName);

            return func;
        }

        protected override IFunction GetElement(ExportRegistry reg)
        {
            return reg.GetFunction(ID);
        }

        public override bool Equals(object obj)
        {
            if (obj is FunctionModel)
            {
                FunctionModel func = (FunctionModel)obj;
                return func.AssemblyName.Equals(AssemblyName)
                    && func.MethodName.Equals(MethodName)
                    && func.ModuleName.Equals(ModuleName);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            UTF8Encoding enc = new UTF8Encoding();
            int sum = 0;
            byte[] bytes = enc.GetBytes(AssemblyName);
            for (int i = 0; i < bytes.Length; i++) 
            {
                sum += 17 * (bytes[i] * (i+1));
            }

            bytes = enc.GetBytes(ModuleName);
            for (int i = 0; i < bytes.Length; i++) 
            {
                sum += 31 * (bytes[i] * (i+1));
            }
            
            bytes = enc.GetBytes(MethodName);
            for (int i = 0; i < bytes.Length; i++) 
            {
                sum += 47 * (bytes[i] * (i+1));
            }

            return sum;
        }
    }

    [XmlRoot(ElementName="task")]
    public class TaskModel : ElementModel<ITask>
    {
        private RefIdModel _startConditionID;
        private RefIdModel _stopConditionID;
        private string _runScript;

        [XmlElement(ElementName = "startOnCondition", Namespace = "")]
        public RefIdModel StartCondition
        {
            get { return _startConditionID; }
            set { _startConditionID = value; }
        }

        [XmlElement(ElementName = "stopOnCondition", Namespace = "")]
        public RefIdModel StopCondition
        {
            get { return _stopConditionID; }
            set { _stopConditionID = value; }
        }

        [XmlElement(ElementName = "runScript", Namespace = "")]
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

    public class ConditionModel : ElementModel<ICondition>
    {
        [XmlElement(ElementName = "task", Namespace="")]
        public RefIdModel Task { get; set; }

        [XmlElement(ElementName = "value", Namespace="")]
        public RefIdModel Value { get; set; }

        [XmlElement(ElementName = "expectedValue", Namespace="")]
        public string ExpectedValue { get; set; }

        [XmlElement(ElementName = "function", Namespace="")]
        public RefIdModel Function { get; set; }

        [XmlElement(ElementName = "state", Namespace="")]
        public string State { get; set; }

        [XmlElement(ElementName = "conditionType", Namespace = "")]
        public string ConditionType { get; set; }

        override public void Import(ProcessModel model, ICondition cond)
        {
            base.Import(model, cond);

            if (cond is TaskCondition)
            {
                TaskCondition taskCond = (TaskCondition)cond;
                Task = CreateTaskReference(model, taskCond.Task);
                State = taskCond.State.ToString();
                ConditionType = "TaskCondition";
            }
            else if (cond is ValueCondition)
            {
                ValueCondition valCond = (ValueCondition)cond;
                Value = CreateValueReference(model, valCond.Value);
                ExpectedValue = valCond.ExpectedValue.ToString();
                Function = CreateFunctionReference(model, valCond.FunctionElement);
                ConditionType = "ValueCondition";
            }
            else
            {
                string className = cond.GetType().Name;
                ConditionType = className.Substring(className.LastIndexOf(".") + 1);
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
        [XmlElement(ElementName="initialValue", Namespace="")]
        public string Value { get; set; }

        [XmlElement(ElementName = "condition", Namespace="")]
        public RefIdModel Condition { get; set; }

        [XmlElement(ElementName = "task", Namespace="")]
        public RefIdModel Task { get; set; }

        [XmlElement(ElementName = "compareValue", Namespace="")]
        public RefIdModel CompareValue { get; set; }

        [XmlElement(ElementName = "valueType", Namespace = "")]
        public string ValueType { get; set; }

        override public void Import(ProcessModel model, IValue v)
        {
            base.Import(model, v);
            Value = v.Value.ToString();
            if (v is AtomicView)
            {
                if (v is ConditionMetView)
                {
                    ICondition cond = ((ConditionMetView)v).Condition;
                    Condition = CreateConditionReference(model, cond);
                    ValueType = "ConditionMetView";
                }
                else if (v is TaskStateView)
                {
                    IRunnable task = ((TaskStateView)v).Task;
                    Task = CreateTaskReference(model, task);
                    ValueType = "TaskStateView";
                }
                else if (v is ValueModifiedView) 
                {
                    IValue compareVal = ((ValueModifiedView)v).CompareValue;
                    CompareValue = CreateValueReference(model, compareVal);
                    ValueType = "ValueModifiedView";
                }
            }
            else
            {
                string className = v.GetType().Name;
                ValueType = className.Substring(className.LastIndexOf(".") + 1);
            }
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
}