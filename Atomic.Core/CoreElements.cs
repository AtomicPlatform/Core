using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Atomic.Core
{
    abstract public class BaseContainer : IContainer
    {
        private List<IProcess> _processList = new List<IProcess>();

        public bool DebugMode { get; set; }

        public System.IO.Stream DebugStream { get; set; }

        public IProcess[] ProcessList
        {
            get { return _processList.ToArray(); }
            protected set
            {
                _processList.Clear();
                _processList.AddRange(value);
            }
        }

        public void AddProcess(IProcess p)
        {
            _processList.Add(p);
            p.Locked = true;
        }

        abstract public void Run();

        abstract public void ExecuteFunction(string functionText);
    }


    abstract public class RunContainer : BaseContainer
    {
        private int _step = 0;

        override public void Run()
        {
            bool done = false;
            while (!done)
            {
                _step++;
                if (DebugMode) WriteDebugMessage("Current step: " + _step + "...");
                List<ITask> activeTasks = new List<ITask>();

                // gather list of all process tasks
                foreach (IProcess p in ProcessList)
                {
                    if (DebugMode) WriteDebugMessage("Process {" + p.Name + "}, CurrentState = " + p.CurrentState.ToString());
                    if (p.CurrentState == TaskState.Running)
                    {
                        activeTasks.AddRange(p.Tasks);
                    }
                }

                // run each task
                foreach (ITask task in activeTasks)
                {
                    foreach (IValue v in task.Values) v.Update();

                    if (task.CurrentState == TaskState.Running
                        || task.CurrentState == TaskState.RunComplete)
                    {
                        ExecuteFunction(task.FunctionText);
                    }
                }

                // update process state
                foreach (IProcess p in ProcessList)
                {
                    if (DebugMode) WriteDebugMessage("Updating process {" + p.Name + "}");
                    p.Update();
                }

                // filter out completed tasks
                List<IProcess> activeList = new List<IProcess>();
                activeList.AddRange(ProcessList.Where(x => x.CurrentState != TaskState.Done));
                ProcessList = activeList.ToArray();

                done = (ProcessList.Length == 0);
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
            set { _locked = value; }
        }

        virtual public void Update() { }

        virtual public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                _id = GenerateID(_name);
            }
        }

        virtual protected string ElementName
        {
            get { return "element"; }
        }
    }

    public class AtomicFunction : AtomicElement, IFunction
    {
        private MethodInfo _meth = null;

        public AtomicFunction() { }

        public AtomicFunction(MethodInfo meth)
        {
            _meth = meth;
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
            // validate parameters
            AssemblyName asmNM = new AssemblyName(assemblyName);
            Assembly asm = Assembly.Load(asmNM);

            TypeInfo info = asm.DefinedTypes.Where(x => x.Name == ModuleName).FirstOrDefault();
            MethodInfo meth = info.DeclaredMethods.Where(x => x.Name == MethodName).FirstOrDefault();

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
            if (obj is AtomicFunction)
            {
                AtomicFunction func = (AtomicFunction)obj;
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
    }

    abstract public class AtomicView : AtomicValue
    {
        abstract public override object Value
        {
            get;
        }
    }

    public class ConditionMetView : AtomicView
    {
        private ICondition _condition = Undefined.Condition;

        public ICondition Condition 
        {
            get { return _condition; }
            set { _condition = value; }
        }

        public override object Value
        {
            get { return Condition.Met(); }
        }
    }

    public class TaskStateView : AtomicView
    {
        private IRunnable _task = Undefined.Task;

        public IRunnable Task 
        {
            get { return _task; }
            set { _task = value; }
        }

        public override object Value
        {
            get { return Task.CurrentState; }
        }
    }

    public class ValueModifiedView : AtomicView
    {
        private IValue _compareValue = Undefined.Value;

        public IValue CompareValue 
        {
            get { return _compareValue; }
            set { _compareValue = value; }
        }

        public override object Value
        {
            get { return CompareValue.Modified; }
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

        abstract public bool Met();

        protected override string ElementName
        {
            get { return "condition"; }
        }
    }
    
    public class StandaloneCondition : AtomicCondition
    {
        static public bool DefaultFunction() { return false; }
        
        public StandaloneCondition()
        {
            Func<bool> func = StandaloneCondition.DefaultFunction;
            FunctionElement = new AtomicFunction(func.GetMethodInfo());
        }
        
        public override bool Met()
        {
            return MetFunction();
        }

        public Func<bool> MetFunction 
        {
            get 
            {
                MethodInfo meth = FunctionElement.Method;
                return (Func<bool>)meth.CreateDelegate(typeof(Func<bool>)); 
            }
            set 
            { 
                FunctionElement = new AtomicFunction(value.GetMethodInfo());
            }
        }
    }

    public class TaskCondition : AtomicCondition
    {
        static public bool DefaultFunction(TaskCondition cond) 
        {
            return cond.Task.CurrentState == cond.State;
        }

        private IRunnable _task = Undefined.Task;
        private TaskState _state = TaskState.Done;
        
        public TaskCondition()
        {
            Func<TaskCondition, bool> func = TaskCondition.DefaultFunction;
            FunctionElement = new AtomicFunction(func.GetMethodInfo());
        }
        
        public override bool Met()
        {
            return MetFunction(this);
        }

        public Func<TaskCondition, bool> MetFunction 
        {
            get 
            {
                MethodInfo meth = FunctionElement.Method;
                return (Func<TaskCondition, bool>)meth.CreateDelegate(typeof(Func<TaskCondition, bool>)); 
            }
            set 
            { 
                FunctionElement = new AtomicFunction(value.GetMethodInfo());
            }
        }

        public IRunnable Task 
        {
            get { return _task; }
            set { _task = value; }
        }

        public TaskState State 
        { 
            get { return _state; }
            set { _state = value; }
        }
    }

    public class ValueCondition : AtomicCondition
    {
        static public bool DefaultFunction(ValueCondition valCond)
        {
            return valCond.Value.Equals(valCond.ExpectedValue);
        }

        private IValue _value = Undefined.Value;
        private IValue _expected = Undefined.Value;

        public ValueCondition()
        {
            Func<ValueCondition, bool> func = ValueCondition.DefaultFunction;
            FunctionElement = new AtomicFunction(func.GetMethodInfo());
        }
        
        public override bool Met()
        {
            return MetFunction(this);
        }

        public Func<ValueCondition, bool> MetFunction 
        {
            get 
            {
                MethodInfo meth = FunctionElement.Method;
                return (Func<ValueCondition, bool>)meth.CreateDelegate(typeof(Func<ValueCondition, bool>)); 
            }
            set 
            { 
                FunctionElement = new AtomicFunction(value.GetMethodInfo());
            }
        }

        public override string Name
        {
            get { return base.Name; }
            set
            {
                base.Name = value;
                if (FunctionElement != null) FunctionElement.Name = base.Name + "_func";
            }
        }

        public IValue Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public IValue ExpectedValue
        {
            get { return _expected; }
            set { _expected = value; }
        }

        /*
        public override bool Met()
        {
            return (bool)MetFunction.Method.Invoke(this, new object[] { Value, ExpectedValue });
        }
        */

        protected override string ElementName
        {
            get { return "function"; }
        }

//        public Func<ValueCondition, bool> MetFunction { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is ValueCondition)
            {
                ValueCondition valCond = (ValueCondition)obj;
                return valCond.FunctionElement.Equals(FunctionElement)
                    && valCond.ExpectedValue.Equals(ExpectedValue)
                    && valCond.Value.Equals(Value);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            int metHash = (FunctionElement == null) ? 13 : FunctionElement.GetHashCode();
            int expectedHash = (ExpectedValue == null) ? 1 : ExpectedValue.GetHashCode();
            int valueHash = (Value == null) ? 1023 : Value.GetHashCode();

            return 17 * metHash + 37 * expectedHash + 3 * valueHash;
        }
    }

    public class RuleCondition : AtomicCondition
    {
        public RuleCondition()
        {
            Conditions = new ICondition[] { };
            MetFunction = CoreFunctions.AllConditionsMet;
        }

        public ICondition[] Conditions { get; set; }

        public Func<ICondition[], bool> MetFunction { get; set; }

        public override bool Met()
        {
            return MetFunction(Conditions);
        }
    }

    public class SequencedCondition : AtomicCondition
    {
        public SequencedCondition()
        {
            Conditions = new ICondition[] { };
            MetFunction = CoreFunctions.DefaultMetFunction;
        }

        private int currentIndex = 0;

        public ICondition[] Conditions { get; set; }

        public Func<ICondition, bool> MetFunction { get; set; }

        public override bool Met()
        {
            if (Conditions.Length == 0) return true;

            bool conditionMet = MetFunction(Conditions[currentIndex]);
            if (conditionMet)
            {
                currentIndex++;
            }
            else
            {
                currentIndex = 0;
            }

            return (currentIndex == Conditions.Length);
        }
    }

    public class AtomicEvent : AtomicElement, IEvent
    {
        private ICondition _startCondition = Undefined.Condition;
        private ICondition _stopCondition = Undefined.Condition;

        public AtomicEvent()
        {
        }

        override public bool Locked
        {
            get { return base.Locked; }
            set
            {
                StartCondition.Locked = value;
                StopCondition.Locked = value;

                base.Locked = value;
            }
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

        protected override string ElementName
        {
            get { return "event"; }
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is AtomicEvent)
            {
                AtomicEvent evt = (AtomicEvent)obj;
                return evt.StartCondition.Equals(StartCondition)
                    && evt.StopCondition.Equals(StopCondition);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            int startHash = (StartCondition == null) ? 1 : StartCondition.GetHashCode();
            int stopHash = (StopCondition == null) ? 5 : StopCondition.GetHashCode();

            return 17 * startHash + 37 * stopHash;
        }
    }

    abstract public class AtomicRunnable : AtomicElement, IRunnable
    {
        private List<IValue> _values = new List<IValue>();
        private TaskFunction _func = CoreFunctions.DefaultRunFunction;
        private TaskState _state = TaskState.Ready;

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

        public TaskState CurrentState
        {
            get { return _state; }
            set { _state = value; }
        }

        abstract public void Run();
    }

    public class AtomicTask : AtomicRunnable, ITask
    {
        private ICondition _startCondition = Undefined.Condition;
        private ICondition _stopCondition = Undefined.Condition;
        private string _funcText = "";

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

        public AtomicTask()
        {
            TaskCondition taskCond = new TaskCondition() { Task = this, State = TaskState.RunComplete };
            StopCondition = taskCond;

            StartCondition = new StandaloneCondition()
            {
                MetFunction = CoreFunctions.DefaultMetFunction
            };

            RunFunction = CoreFunctions.DefaultRunFunction;
        }

        public string FunctionText {
            get { return _funcText; }
            set { _funcText = value; } 
        }

        override public void Run()
        {
            foreach (IValue v in Values)
            {
                v.Update();
            }

            switch (CurrentState)
            {
                case TaskState.Running:
                case TaskState.RunComplete:
                    RunFunction(this);
                    break;
            }
        }

        override public void Update()
        {
            switch (CurrentState)
            {
                case TaskState.Ready:
                    if (StartCondition == null || StartCondition.Met())
                    {
                        CurrentState = TaskState.Running;
                    }
                    break;
                case TaskState.Running:
                case TaskState.RunComplete:
                    CurrentState = TaskState.RunComplete;

                    if (StopCondition.Met())
                    {
                        CurrentState = TaskState.Done;
                    }
                    break;
                case TaskState.Done:
                    CurrentState = TaskState.Ready;
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
        private IEvent _startEvent = new AtomicEvent();
        private IEvent _startEventDefault = new AtomicEvent();
        private IEvent _stopEvent = new AtomicEvent();
        private IEvent _stopEventDefault = new AtomicEvent();

        private List<IEvent> _events = new List<IEvent>();
        private List<ITask> _tasks = new List<ITask>();

        public AtomicProcess()
        {
            InitializeStartEvent(StartEvent);
            InitializeStartEvent(DefaultStartEvent);

            InitializeStopEvent(StopEvent);
            InitializeStopEvent(DefaultStopEvent);
        }

        private void InitializeStartEvent(IEvent evt)
        {
            evt.Name = "_start";
            IValue startView = new TaskStateView() 
            { 
                Task = this, 
                Name = "_startEvent_startView" 
            };

            evt.StartCondition = new ValueCondition()
            {
                Name = "_startEvent_start",
                Value = startView,
                ExpectedValue = new AtomicValue() { Value = TaskState.Ready }
            };

            evt.StopCondition = new ValueCondition()
            {
                Name = "_startEvent_stop",
                Value = new TaskStateView() { Task = this, Name = "_startEvent_stopView" },
                ExpectedValue = new AtomicValue() { Value = TaskState.Active }
            };
        }

        private void InitializeStopEvent(IEvent evt)
        {
            evt.Name = "_stop";
            IValue startView = new TaskStateView()
            {
                Task = this,
                Name = "_stopEvent_startView"
            };

            evt.StartCondition = new ValueCondition()
            {
                Name = "_stopEvent_start",
                Value = startView,
                ExpectedValue = new AtomicValue() { Value = TaskState.Running }
            };

            evt.StopCondition = new ValueCondition()
            {
                Name = "_stopEvent_stop",
                Value = new TaskStateView() { Task = this, Name = "_startEvent_stopView" },
                ExpectedValue = new AtomicValue() { Value = TaskState.RunComplete }
            };
        }

        public IEvent StartEvent 
        {
            get { return _startEvent; }
            private set { _startEvent = value; }
        }

        public IEvent DefaultStartEvent
        {
            get { return _startEventDefault; }
            private set { _startEventDefault = value; }
        }

        public IEvent StopEvent
        {
            get { return _stopEvent; }
            private set { _stopEvent = value; }
        }

        public IEvent DefaultStopEvent
        {
            get { return _stopEventDefault; }
            private set { _stopEventDefault = value; }
        }

        public IEvent[] Events 
        {
            get { return _events.ToArray(); }
            set 
            { 
                _events.Clear(); 
                _events.AddRange(value); 
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
                case TaskState.Ready:
                    if (StartEvent.StartCondition.Met())
                    {
                        CurrentState = TaskState.Active;
                        StartEvent.Update();
                    }
                    break;
                case TaskState.Active:
                    if (StartEvent.StopCondition.Met())
                    {
                        CurrentState = TaskState.Running;
                        StartEvent.Update();
                    }
                    break;
                case TaskState.Running:
                    foreach (ITask task in Tasks)
                    {
                        task.Update();
                    }

                    if (StopEvent.StartCondition.Met())
                    {
                        CurrentState = TaskState.RunComplete;
                        StopEvent.Update();
                    }
                    break;
                case TaskState.RunComplete:
                case TaskState.Stopping:
                    if (StopEvent.StopCondition.Met())
                    {
                        CurrentState = TaskState.Done;
                    }
                    else
                    {
                        CurrentState = TaskState.Stopping;
                    }
                    break;
            }
        }

        protected override string ElementName
        {
            get { return "program"; }
        }
    }
}
