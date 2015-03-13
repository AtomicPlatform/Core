﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Atomic.Core
{
    abstract public class BaseContainer : IContainer
    {
        private List<ITask> _taskList = new List<ITask>();

        public bool DebugMode { get; set; }

        public System.IO.Stream DebugStream { get; set; }

        public ITask[] Tasks
        {
            get { return _taskList.ToArray(); }
            protected set
            {
                _taskList.Clear();
                _taskList.AddRange(value);
            }
        }

        public void AddTask(ITask task)
        {
            _taskList.Add(task);
            task.Locked = true;
        }

        abstract public void Run();

        abstract public string ExecuteTask(ITask task);

        abstract public void HandleError(string errorText);

        public string FormatResponse(IDictionary<string, object> response)
        {
            if (response.Count == 0) return "";

            StringBuilder builder = new StringBuilder();
            builder.Append("{ ");
            foreach (KeyValuePair<string, object> keyPair in response)
            {
                builder.Append("\"" + keyPair.Key + "\": " + ToJsonString(keyPair.Value));
                if (keyPair.Key != response.Last().Key) builder.Append(", ");
            }

            builder.Append(" }");

            return builder.ToString();
        }

        private string ToJsonString(object o)
        {
            if (o is string)
            {
                return "\"" + (string)o + "\"";
            }
            else
            {
                return "\"" + o.ToString() + "\"";
            }
        }
    }


    abstract public class RunContainer : BaseContainer
    {
        private int _step = 0;

        override public void Run()
        {
            _step++;
            if (DebugMode) WriteDebugMessage("Current step: " + _step + "...");
            List<ITask> activeTasks = new List<ITask>();

            // gather list of all process tasks
            foreach (ITask task in Tasks)
            {
                if (DebugMode) WriteDebugMessage("Task {" + task.Name + "}, CurrentState = " + task.CurrentState.ToString());
                if (task.CurrentState == RunState.Running)
                {
                    activeTasks.Add(task);
                }
            }

            // run each task
            foreach (ITask task in activeTasks)
            {
                foreach (IValue v in task.Values) v.Update();

                if (task.CurrentState == RunState.Running
                    || task.CurrentState == RunState.RunComplete)
                {
                    string result = ExecuteTask(task);
                    HandleError(result);
                }
            }

            // update process state
            foreach (ITask task in Tasks)
            {
                if (DebugMode) WriteDebugMessage("Updating process {" + task.Name + "}");
                task.Update();
            }
        }

        private void WriteDebugMessage(string message)
        {
            UTF8Encoding enc = new UTF8Encoding();
            byte[] buffer = enc.GetBytes(message + "\n");

            DebugStream.Write(buffer, 0, buffer.Length);
            DebugStream.Flush();
        }
    }

    public class AtomicElement : IElement
    {
        private string _id = "";
        private string _name = "";
        private bool _locked = false;

        static public string GenerateID(string name)
        {
            if (name == null) name = "";
            return name.Trim().ToLower().Replace(' ', '_');
        }

        public AtomicElement()
        {
            Name = ElementName + GetHashCode();
        }

        public string ID
        {
            get { return _id; }
        }

        virtual public bool Locked
        {
            get { return _locked; }
            set
            {
                if (IsValid) _locked = value;
            }
        }

        virtual public void Update() { }

        virtual public string Name
        {
            get { return _name; }
            set
            {
                if (Locked) return;

                if (value == null) value = "";
                value = value.Trim();
                if (value.Length == 0) value = ElementName + GetHashCode();

                _name = value;
                _id = GenerateID(_name);
            }
        }

        virtual protected string ElementName
        {
            get { return "element"; }
        }

        virtual public bool IsValid
        {
            get { return true; }
        }
    }

    public class AtomicFunction : AtomicElement, IFunction
    {
        private MethodInfo _meth = null;

        public AtomicFunction() { }

        public AtomicFunction(MethodInfo meth)
        {
            if (meth == null) return;

            if (meth.IsStatic && meth.IsPublic)
            {
                _meth = meth;
            }
            else
            {
                _meth = null;
            }
        }

        public string AsmName
        {
            get
            {
                if (_meth == null) return "";
                return _meth.Module.Assembly.FullName;
            }
        }

        public string ModuleName
        {
            get
            {
                if (_meth == null) return "";
                return _meth.DeclaringType.Name;
            }
        }

        public string MethodName
        {
            get
            {
                if (_meth == null) return "";
                return _meth.Name;
            }
        }

        public void SetProperties(string assemblyName, string moduleName, string methodName)
        {
            if (assemblyName == null || moduleName == null || methodName == null)
            {
                _meth = null;
                return;
            }

            // validate parameters
            AssemblyName asmNM = null;
            try
            {
                asmNM = new AssemblyName(assemblyName);
            }
            catch (Exception)
            {
                _meth = null;
                return;
            }

            Assembly asm = null;

            try
            {
                asm = Assembly.Load(asmNM);
            }
            catch (Exception)
            {
                _meth = null;
                return;
            }

            TypeInfo info = asm.DefinedTypes.Where(x => x.Name == moduleName).FirstOrDefault();
            if (info == null)
            {
                _meth = null;
                return;
            }

            MethodInfo meth = info.DeclaredMethods.Where(x => x.Name == methodName).FirstOrDefault();

            _meth = meth;
        }

        public MethodInfo Method
        {
            get { return _meth; }
        }

        protected override string ElementName
        {
            get { return "function"; }
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is IFunction)
            {
                IFunction func = (IFunction)obj;
                return func.AsmName.Equals(AsmName)
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
            return AsmName.GetHashCode() * MethodName.GetHashCode() * ModuleName.GetHashCode();
        }
    }

    /*
    public class AtomicMessage : AtomicElement, IMessage
    {
        private List<IValue> _values = new List<IValue>();

        public IValue[] Values
        {
            get { return _values.ToArray(); }
            set
            {
                _values.Clear();
                _values.AddRange(value);
            }
        }
    }
    */

    public class AtomicValue : AtomicElement, IValue
    {
        private object _publicValue = null;
        private object _internalValue = null;

        private bool _modified = false;

        public bool Modified
        {
            get { return _modified; }
            private set { _modified = value; }
        }

        virtual public object Value
        {
            get { return _publicValue; }
            set
            {
                if (Locked)
                {
                    Modified = true;
                }
                else
                {
                    _publicValue = value;
                }

                _internalValue = value;
            }
        }

        override public void Update()
        {
            if (Modified)
            {
                _publicValue = _internalValue;
                Modified = false;
            }
        }

        protected override string ElementName
        {
            get { return "value"; }
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is IValue)
            {
                AtomicValue val = (AtomicValue)obj;
                if (val.Value == null)
                {
                    return (Value == null);
                }
                else
                {
                    return val.Value.Equals(Value);
                }
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return (Value == null) ? 0 : Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class ParameterValue : AtomicValue, IParameter
    {
        private bool _isInput = true;
        private bool _isRequired = true;

        public bool InputParameter
        {
            get { return _isInput; }
            set { _isInput = value; }
        }

        public bool Required
        {
            get { return _isRequired; }
            set { _isRequired = value; }
        }
    }

    abstract public class AtomicView : AtomicValue
    {
        abstract public override object Value
        {
            get;
        }
    }

    abstract public class AtomicCondition : AtomicElement, ICondition
    {
        private IFunction _funcObj = Undefined.Function;

        public IFunction FunctionElement
        {
            get { return _funcObj; }
            set { _funcObj = value; }
        }

        abstract public bool Met { get; }

        protected override string ElementName
        {
            get { return "condition"; }
        }
    }


    abstract public class AtomicEvent : AtomicElement, IEvent
    {
        private ICondition _condition = Undefined.Condition;
        private IProcess _process = null;

        public AtomicEvent()
        {
        }

        override public bool Locked
        {
            get { return base.Locked; }
            set
            {
                StartCondition.Locked = value;

                base.Locked = value;
            }
        }

        public IProcess Process
        {
            get { return _process; }
            set
            {
                _process = value;
                InitializeStartCondition();
            }
        }

        virtual public ICondition StartCondition
        {
            get { return _condition; }
            protected set { _condition = value; }
        }

        protected override string ElementName
        {
            get { return "event"; }
        }

        abstract protected void InitializeStartCondition();

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is AtomicEvent)
            {
                AtomicEvent evt = (AtomicEvent)obj;
                return evt.StartCondition.Equals(StartCondition);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            int startHash = (StartCondition == null) ? 1 : StartCondition.GetHashCode();

            return 17 * startHash;
        }
    }

    abstract public class AtomicRunnable : AtomicElement, IRunnable
    {
        private List<IValue> _values = new List<IValue>();
        private TaskFunction _func = CoreFunctions.DefaultRunFunction;
        private RunState _state = RunState.Ready;

        public IValue[] Values
        {
            get { return _values.ToArray(); }
            set
            {
                _values.Clear();
                _values.AddRange(value);
            }
        }

        public TaskFunction RunFunction
        {
            get { return _func; }
            set { _func = value; }
        }

        public RunState CurrentState
        {
            get { return _state; }
            set { _state = value; }
        }

        public IValue GetValue(string name)
        {
            string nameID = AtomicElement.GenerateID(name);
            IValue matchValue = _values.Where(x => x.ID == nameID).FirstOrDefault();

            return (matchValue == null) ? Undefined.Value : matchValue;
        }

        abstract public void Run();
    }

    abstract public class AtomicTask : AtomicRunnable, ITask
    {
        private ICondition _startCondition = Undefined.Condition;
        private ICondition _stopCondition = Undefined.Condition;
        private string _funcText = "";
        private IValue _runResult = new AtomicValue();

        public AtomicTask()
        {
            TaskCondition taskCond = new TaskCondition() { Task = this, State = RunState.Running };
            StopCondition = taskCond;

            StartCondition = new StandaloneCondition()
            {
                MetFunction = CoreFunctions.DefaultMetFunction
            };

            RunFunction = CoreFunctions.DefaultRunFunction;
        }

        public ICondition StartCondition
        {
            get { return _startCondition; }
            set { _startCondition = value; }
        }

        public ICondition StopCondition
        {
            get { return _stopCondition; }
            set { _stopCondition = value; }
        }

        public string FunctionText
        {
            get { return _funcText; }
            set { _funcText = value; }
        }

        public IValue RunResult
        {
            get { return _runResult; }
        }

        override public void Run()
        {
            foreach (IValue v in Values)
            {
                v.Update();
            }

            switch (CurrentState)
            {
                case RunState.Running:
                    RunFunction(this);
                    break;
            }
        }

        override public void Update()
        {
            switch (CurrentState)
            {
                case RunState.Ready:
                    if (StartCondition == null || StartCondition.Met)
                    {
                        CurrentState = RunState.Running;
                    }
                    break;
                case RunState.Running:
                    if (StopCondition.Met)
                    {
                        CurrentState = RunState.Done;
                    }
                    break;
                case RunState.Done:
                    CurrentState = RunState.Ready;
                    break;
            }
        }

        protected override string ElementName
        {
            get { return "task"; }
        }
    }

    public class AtomicActivity : AtomicTask, IActivity
    {
        private List<ITask> _tasks = new List<ITask>();

        public AtomicActivity()
        {
        }

        public override void Run()
        {
            base.Run();

            foreach (ITask t in Tasks)
            {
                t.Run();
            }
        }

        public override void Update()
        {
            base.Update();

            foreach (ITask t in Tasks)
            {
                t.Update();
            }
        }

        public ITask[] Tasks
        {
            get { return _tasks.ToArray(); }
            set { _tasks.Clear(); _tasks.AddRange(value); }
        }

        public override bool Locked
        {
            get { return base.Locked; }
            set
            {
                base.Locked = value;

                foreach (ITask t in Tasks)
                {
                    t.Locked = value;
                }
            }
        }
    }

    public class AtomicProcess : AtomicRunnable, IProcess
    {
        private StartEvent _startEvent = new StartEvent();
        private StopEvent _stopEvent = new StopEvent();

        private List<IEvent> _events = new List<IEvent>();
        private List<ITask> _tasks = new List<ITask>();

        private ICondition _doneCondition = Undefined.Condition;
        private IDictionary<Type, IContainer> _containers = new Dictionary<Type, IContainer>();

        static public ICondition TaskStateCondition(IRunnable task, RunState runState)
        {
            return new ValueCondition()
            {
                Name = "Is Process Running",
                MetFunction = ValueCondition.EqualsFunction,
                Value = new TaskStateView() { Name = "Current " + task.Name + " State", Task = task },
                ExpectedValue = new AtomicValue()
                {
                    Name = "Expected Process State",
                    Value = runState
                }
            };
        }

        public AtomicProcess()
        {
            _startEvent.Process = this;
            _stopEvent.Process = this;
        }

        public IEvent StartEvent
        {
            get { return _startEvent; }
        }

        public IEvent StopEvent
        {
            get { return _stopEvent; }
        }

        public IEvent[] Events
        {
            get { return _events.ToArray(); }
            set
            {
                _events.Clear();
                foreach (IEvent evt in value)
                {
                    _events.Add(evt);
                    evt.Process = this;
                }
            }
        }

        public ITask[] Tasks
        {
            get { return _tasks.ToArray(); }
            set
            {
                _tasks.Clear();
                _tasks.AddRange(value);
            }
        }

        public ITask GetTask(string name)
        {
            string nameID = AtomicElement.GenerateID(name);
            ITask matchTask = _tasks.Where(x => x.ID == nameID).FirstOrDefault();

            return (matchTask == null) ? Undefined.Task : matchTask;
        }

        public IContainer GetContainer(Type taskType)
        {
            if (_containers.ContainsKey(taskType))
            {
                return _containers[taskType];
            }
            else
            {
                return Undefined.Container;
            }
        }

        public void SetContainer(Type taskType, IContainer container)
        {
            _containers[taskType] = container;
        }

        public override bool Locked
        {
            get { return base.Locked; }
            set
            {
                StartEvent.Locked = value;
                StopEvent.Locked = value;
                foreach (ITask task in Events)
                {
                    task.Locked = value;
                }

                base.Locked = value;
            }
        }

        override public void Run()
        {
        }

        override public void Update()
        {
            switch (CurrentState)
            {
                case RunState.Ready:
                    if (StartEvent.StartCondition.Met)
                    {
                        CurrentState = RunState.Starting;
                        StartEvent.Update();
                    }
                    break;
                case RunState.Starting:
                    ITask[] tasks = GetStartingTasks();
                    if (tasks.Length == 0)
                    {
                        CurrentState = RunState.Running;
                    }
                    break;
                case RunState.Running:
                    if (StopEvent.StartCondition.Met)
                    {
                        CurrentState = RunState.RunComplete;
                        StopEvent.Update();
                    }
                    break;
                case RunState.RunComplete:
                    CurrentState = RunState.Done;
                    break;
            }
        }

        public ICondition DoneCondition
        {
            get { return _doneCondition; }
            set
            {
                _doneCondition = value;
                _doneCondition.Name = "_done";
                ((StopEvent)StopEvent).UpdateCondition(_doneCondition);
            }
        }

        protected override string ElementName
        {
            get { return "program"; }
        }

        private ITask[] GetStartingTasks()
        {
            List<ITask> taskList = new List<ITask>();
            taskList.AddRange(GetRunningTasks());
            taskList.AddRange(Tasks.Where(x => x.StartCondition.Met && x.CurrentState == RunState.Ready));

            return taskList.ToArray();
        }

        private ITask[] GetRunningTasks()
        {
            List<ITask> taskList = new List<ITask>();
            taskList.AddRange(Tasks.Where(x => x.CurrentState != RunState.Ready && x.CurrentState != RunState.Done));

            return taskList.ToArray();
        }
    }

    public class AtomicMessage : AtomicElement, IMessage
    {
        private List<IParameter> _paramList = new List<IParameter>();

        public IParameter[] Parameters
        {
            get { return _paramList.ToArray(); }
            set
            {
                _paramList.Clear();
                _paramList.AddRange(value);
            }
        }

        public IParameter GetParameter(string name)
        {
            string paramID = AtomicElement.GenerateID(name);
            IParameter para = _paramList.Where(x => x.ID == paramID).FirstOrDefault();

            return para;
        }

        public string[] ParameterNames
        {
            get { throw new NotImplementedException(); }
        }
    }
}
