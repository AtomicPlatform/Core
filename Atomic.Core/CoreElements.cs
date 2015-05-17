using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Atomic.Core
{
    public delegate bool ContainerCommand(string commandText);

    abstract public class AtomicContainer : AtomicElement, IContainer
    {
        private IDictionary<IProcess, MethodInfo> _processes = new Dictionary<IProcess, MethodInfo>();
        private IDictionary<ITask, MethodInfo> _tasks = new Dictionary<ITask, MethodInfo>();

        static protected string ParseTask(IRunnable task, out string[] tokens)
        {
            tokens = task.RunFunction.FunctionTokens;

            // find parameters and replace with id values
            tokens[1] = SubstituteValues(tokens[1], task);

            // display the final text
            return UnQuoteText(tokens[1]);
        }

        static protected string SubstituteValues(string text, IRunnable task)
        {
            StringBuilder buffer = new StringBuilder(text);
            // do value substitution
            foreach (IValue inputValue in task.Inputs)
            {
                TextView view = (TextView)inputValue;
                buffer.Replace("$[" + view.ID + "]", view.ViewValue);
            }

            return buffer.ToString();
        }

        static protected string UnQuoteText(string text)
        {
            return text.Substring(1, text.Length - 2);
        }

        public ITask[] Tasks
        {
            get { return _tasks.Keys.ToArray(); }
        }

        public IProcess[] Processes
        {
            get { return _processes.Keys.ToArray(); }
        }

        public void Add(IProcess process, ITask[] tasks = null)
        {
            if (!process.Locked) return;

            if (!_processes.ContainsKey(process))
            {
                RegisterProcess(process);

                if (tasks == null) tasks = process.Tasks;
                foreach (ITask task in tasks)
                {
                    RegisterTask(task);
                }
            }
        }

        public void Remove(IProcess process)
        {
            foreach (ITask task in process.Tasks)
            {
                if (_tasks.ContainsKey(task))
                {
                    _tasks.Remove(task);
                }
             }

            _processes.Remove(process);
        }

        public void Run()
        {
            IProcess[] runProcesses = GetRunnableProcesses();
            foreach (IProcess process in runProcesses)
            {
                RunProcess(process);
            }

            ITask[] runTasks = GetRunnableTasks();
            foreach (ITask task in runTasks)
            {
                RunTask(task);
            }
        }

        protected void RunTask(ITask task)
        {
            MethodInfo meth = _tasks[task];
            meth.Invoke(null, new object[] { task });
        }

        protected void RunProcess(IProcess process)
        {
            MethodInfo meth = _processes[process];
            meth.Invoke(null, new object[] { process });
        }

        virtual protected MethodInfo GenerateMethod(IFunction func) 
        { 
            return AtomicRunnable.DefaultFunction.GetMethodInfo(); 
        }

        virtual protected MethodInfo GenerateMethod(IProcess process) 
        { 
            return AtomicRunnable.DefaultFunction.GetMethodInfo(); 
        }

        virtual protected IProcess[] GetRunnableProcesses()
        {
            return new IProcess[0];
        }

        virtual protected ITask[] GetRunnableTasks()
        {
            return Tasks.Where(x => x.CurrentState == RunState.Running).ToArray();
        }

        protected override string ElementName
        {
            get { return "container"; }
        }

        private void RegisterProcess(IProcess process)
        {
            MethodInfo meth = GenerateMethod(process);
            if (meth == null) meth = AtomicRunnable.DefaultFunction.GetMethodInfo();

            _processes[process] = meth; 
        }

        private void RegisterTask(ITask task)
        {
            MethodInfo meth = GenerateMethod(task.RunFunction);
            if (meth == null) meth = AtomicRunnable.DefaultFunction.GetMethodInfo();

            _tasks[task] = meth;
        }
    };

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
        private string _functionText = "";
        private string[] _functionTokens = new string[0];

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

        public AtomicFunction(string assemblyName, string moduleName, string methodName)
        {

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
            set { _meth = value; }
        }

        public string FunctionText
        {
            get { return _functionText; }
            set
            {
                _functionText = value;
                _functionTokens = SplitFunction();
            }
        }

        public string[] FunctionTokens
        {
            get { return _functionTokens; }
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

        private string[] SplitFunction()
        {
            List<string> tokenList = new List<string>();

            // this is very memory inefficient but regex just isn't working for me...
            string text = _functionText.Trim();
            int startPos = 0;
            int endPos = 1;

            while (text.Length > 0)
            {
                endPos = 1;
                if (text[0] == '"')
                {
                    // look for ending quote
                    bool found = false;
                    while (!found)
                    {
                        endPos = text.IndexOf('"', endPos);
                        if (endPos == -1) break;

                        if (text[endPos - 1] == '\\') continue;
                        found = true;
                    }

                    if (found)
                    {
                        text = FormatReferences(text);
                        tokenList.Add(text.Substring(startPos, endPos + 1));
                        text = text.Substring(endPos + 1).TrimStart();
                    }
                    else
                    {
                        // no ending quote, ignore
                        text = "";
                    }
                }
                else
                {
                    // look for next space
                    endPos = text.IndexOf(' ');
                    if (endPos > 0)
                    {
                        tokenList.Add(text.Substring(0, endPos));
                        text = text.Substring(endPos + 1);
                    }
                    else
                    {
                        // last token
                        tokenList.Add(text);
                        text = "";
                    }
                }
            }

            return tokenList.ToArray();
        }

        private string FormatReferences(string text)
        {
            int startPos = 0;
            int endPos = 0;

            while (startPos >= 0)
            {
                startPos = text.IndexOf("${", startPos);
                if (startPos >= 0) endPos = text.IndexOf('}', startPos);
                if (startPos >= 0 && endPos >= 0)
                {
                    int posLength = endPos - startPos + 1;
                    string variableName = text.Substring(startPos + 2, posLength - 3);
                    string variableID = AtomicElement.GenerateID(variableName);

                    text = text.Remove(startPos, posLength);
                    text = text.Insert(startPos, "${" + variableID + "}");

                    startPos = endPos + 1;
                }
            }

            return text;
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
                if (_publicValue == _internalValue)
                {
                    Modified = false;
                }
                else
                {
                    _publicValue = _internalValue;
                }
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

    abstract public class AtomicView : AtomicValue
    {
        public override object Value
        {
            get { return null; }
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

        public override string Name
        {
            get { return base.Name; }
            set
            {
                base.Name = value;
                if (FunctionElement != null) FunctionElement.Name = base.Name + "_func";
            }
        }
    }


    abstract public class AtomicEvent : AtomicCondition, IEvent
    {
        private IProcess _process = null;

        public AtomicEvent()
        {
        }

        public IProcess Process
        {
            get { return _process; }
            set
            {
                _process = value;
            }
        }

        protected override string ElementName
        {
            get { return "event"; }
        }
    }

    abstract public class AtomicRunnable : AtomicElement, IRunnable
    {
        static public TaskFunction DefaultFunction = DoNothing;

        private static void DoNothing(IRunnable task) { }

        // problem - entered state triggers twice, once as entering and once for exiting....
        // need to implement something like the resetvalues of AtomicProcess
        // for now, disable...
        /*
        static public ICondition EnteredStateCondition(IRunnable task, RunState runState)
        {
            return new RuleCondition()
            {
                Conditions = new ICondition[] {
                    new ValueCondition() {
                        Value = new TaskCurrentStateView() { Task = task }, 
                        ExpectedValue = new AtomicValue() { Value = runState }, 
                        MetFunction = ValueCondition.EqualsFunction
                    }, 
                    new ValueCondition() { 
                        Value = new TaskStateModifiedView() { Task = task }, 
                        ExpectedValue = new AtomicValue() { Value = true }, 
                        MetFunction = ValueCondition.EqualsFunction
                    }
                }, 
                MetFunction = RuleCondition.AllConditionsMet
            };
        }
        */
        static public ICondition AtStateCondition(IRunnable task, RunState runState)
        {
            return new ValueCondition()
            {
                Name = "Is Process Running",
                MetFunction = ValueCondition.EqualsFunction,
                Value = new TaskCurrentStateView() { Name = "Current " + task.Name + " State", Task = task },
                ExpectedValue = new AtomicValue()
                {
                    Name = "Expected Process State",
                    Value = runState
                }
            };
        }


        private IDictionary<string, IValue> _outputs = new Dictionary<string, IValue>();
        private IDictionary<string, IValueView> _inputs = new Dictionary<string, IValueView>();
        private IFunction _func = Undefined.Function;
        private RunState _state = RunState.Ready;
        private RunState _oldState = RunState.Ready;

        public IProcess Process { get; set; }

        public IFunction RunFunction
        {
            get { return _func; }
            set { _func = value; }
        }

        public IValueView[] Inputs
        {
            get { return _inputs.Values.ToArray(); }
            set
            {
                _inputs.Clear();
                foreach (IValueView v in value)
                {
                    _inputs.Add(v.ID, v);
                }
            }
        }

        public IValueView GetInput(string name)
        {
            string id = AtomicElement.GenerateID(name);
            return (_inputs.ContainsKey(id)) ? _inputs[id] : Undefined.View;
        }

        public IValue[] Outputs
        {
            get { return _outputs.Values.ToArray(); }
            set
            {
                _outputs.Clear();
                foreach (IValue v in value)
                {
                    _outputs.Add(v.ID, v);
                }
            }
        }

        public IValue GetOutput(string name)
        {
            string id = AtomicElement.GenerateID(name);
            return (_outputs.ContainsKey(id)) ? _outputs[id] : Undefined.Value;
        }

        public RunState CurrentState
        {
            get { return _state; }
            set { _state = value; }
        }

        virtual public void Run()
        {
            MethodInfo meth = _func.Method;
            if (meth == null) return;
            meth.Invoke(null, new object[] { this });
        }

        virtual public void Cancel()
        {
            CurrentState = RunState.Done;
        }

        public override bool Locked
        {
            get { return base.Locked; }
            set
            {
                base.Locked = value;
                if (base.Locked != value) return;

                foreach (IValueView v in _inputs.Values)
                {
                    v.Locked = value;
                }

                foreach (IValue v in _outputs.Values)
                {
                    v.Locked = value;
                }
            }
        }

        public override void Update()
        {
            base.Update();
            _oldState = CurrentState;
        }

        public bool Modified
        {
            get { return _oldState == CurrentState; }
        }
    }

    abstract public class AtomicTask : AtomicRunnable, ITask
    {
        private ICondition _startCondition = Undefined.Condition;
        private ICondition _stopCondition = Undefined.Condition;

        public AtomicTask()
        {
            TaskCondition taskCond = new TaskCondition() { Task = this, State = RunState.RunComplete };
            StopCondition = taskCond;

            StartCondition = new StandaloneCondition()
            {
                MetFunction = CoreFunctions.DefaultMetFunction
            };

            //RunFunction = CoreFunctions.DefaultRunFunction;
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

        override public void Run()
        {
            if (CurrentState == RunState.Running)
            {
                base.Run();
            }
        }

        override public void Update()
        {
            base.Update();

            switch (CurrentState)
            {
                case RunState.Ready:
                    if (StartCondition == Undefined.Condition || StartCondition.Met)
                    {
                        CurrentState = RunState.Running;
                    }

                    break;
                case RunState.Running:
                    CurrentState = RunState.RunComplete;

                    // update the outputs 
                    foreach (IValue v in Outputs)
                    {
                        v.Update();
                    }

                    break;
                case RunState.RunComplete:
                    CurrentState = (StopCondition.Met) ? RunState.Done : RunState.Running;

                    break;
                case RunState.Done:
                    CurrentState = RunState.Ready;
                    break;
            }
        }

        public override bool Locked
        {
            get { return base.Locked; }
            set
            {
                base.Locked = value;
                if (base.Locked != value) return;
            }
        }
        protected override string ElementName
        {
            get { return "task"; }
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

        public AtomicProcess()
        {
            _startEvent.Process = this;
            _stopEvent.Process = this;
            DoneCondition = AtomicProcess.AtStateCondition(this, RunState.Running);
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

        public IEvent GetEvent(string name)
        {
            string key = AtomicElement.GenerateID(name);
            IEvent evt = _events.Where(x => x.ID == key).FirstOrDefault();

            return (evt == null) ? Undefined.Event : evt;
        }

        public ITask[] Tasks
        {
            get { return _tasks.ToArray(); }
            set
            {
                _tasks.Clear();

                foreach (ITask task in value)
                {
                    _tasks.Add(task);
                    task.Process = this;
                }

            }
        }

        public ITask GetTask(string name)
        {
            string nameID = AtomicElement.GenerateID(name);
            ITask matchTask = _tasks.Where(x => x.ID == nameID).FirstOrDefault();

            return (matchTask == null) ? Undefined.Task : matchTask;
        }

        public override bool Locked
        {
            get { return base.Locked; }
            set
            {
                StartEvent.Locked = value;
                StopEvent.Locked = value;

                foreach (IEvent evt in _events)
                {
                    evt.Locked = value;
                }

                foreach (ITask task in _tasks)
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
            base.Update();

            switch (CurrentState)
            {
                case RunState.Ready:
                    if (StartEvent.Met)
                    {
                        CurrentState = RunState.Starting;
                        StartEvent.Update();
                    }
                    break;
                case RunState.Starting:
                    ITask[] startingTasks = GetActiveTasks();
                    if (startingTasks.Length == 0)
                    {
                        CurrentState = RunState.Running;
                    }
                    else
                    {
                        foreach (ITask task in startingTasks)
                        {
                            task.Update();
                        }
                    }

                    break;
                case RunState.Running:
                    if (StopEvent.Met)
                    {
                        CurrentState = RunState.RunComplete;
                    }
                    foreach (ITask task in Tasks)
                    {
                        task.Update();
                    }

                    break;
                case RunState.RunComplete:
                    List<ITask> runningTasks = new List<ITask>();
                    runningTasks.AddRange(Tasks.Where(
                        x => x.CurrentState == RunState.Running || x.CurrentState == RunState.RunComplete
                    ));

                    if (runningTasks.Count == 0)
                    {
                        CurrentState = RunState.Stopping;
                        StopEvent.Update();
                    }
                    foreach (ITask task in runningTasks)
                    {
                        if (task.StopCondition.Met)
                        {
                            task.Update();
                        }
                        else
                        {
                            task.Cancel();
                        }
                    }

                    break;
                case RunState.Stopping:
                    ITask[] stoppingTasks = GetActiveTasks();
                    if (stoppingTasks.Length == 0)
                    {
                        CurrentState = RunState.Done;
                    }
                    else
                    {
                        foreach (ITask task in stoppingTasks)
                        {
                            task.Update();
                        }
                    }
                    break;
                case RunState.Done:
                    // stay in the state
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
            }
        }

        public void ResetValues()
        {
            foreach (IValue v in Outputs.Where(x => x.Modified))
            {
                v.Update();
            }

            foreach (ITask t in Tasks)
            {
                foreach (IValue v in t.Outputs.Where(x => x.Modified))
                {
                    v.Update();
                }
            }
        }

        public override bool IsValid
        {
            get
            {
                // to do: this needs to walk the tree
                return base.IsValid;
            }
        }

        protected override string ElementName
        {
            get { return "program"; }
        }

        private ITask[] GetActiveTasks()
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
}
