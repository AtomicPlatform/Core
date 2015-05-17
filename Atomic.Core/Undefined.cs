using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atomic.Core
{
    static public class Undefined
    {
        static private ICondition _condition = new UndefinedCondition();
        static private IFunction _function = new UndefinedFunction();
        static private IEvent _event = new UndefinedEvent();
        static private ITask _task = new UndefinedTask();
        static private IValue _value = new UndefinedValue();
        static private IValueView _view = new UndefinedView();
        static private IContainer _container = new UndefinedContainer();
        static private IProcess _process = new UndefinedProcess();

        static public ICondition Condition
        {
            get { return _condition; }
        }

        static public IContainer Container
        {
            get { return _container; }
        }

        static public IEvent Event
        {
            get { return _event; }
        }

        static public IFunction Function
        {
            get { return _function; }
        }

        static public IProcess Process
        {
            get { return _process; }
        }

        static public ITask Task
        {
            get { return _task; }
        }

        static public IValue Value
        {
            get { return _value; }
        }

        static public IValueView View
        {
            get { return _view; }
        }
    }

    class UndefinedElement : IElement
    {

        public string ID 
        {
            get { return "#undef"; }
        }

        public string Name
        {
            get { return "#undef"; }
            set { }
        }

        public void Update()
        {
        }

        public bool Locked
        {
            get { return true; }
            set { }
        }

        public bool IsValid
        {
            get { return false; }
        }
    }

    class UndefinedCondition : UndefinedElement, ICondition 
    {
        public bool Met 
        { 
            get { return false; } 
        }
    }

    class UndefinedContainer : UndefinedElement, IContainer
    {
        private ITask[] _tasks = new ITask[0];

        public ITask[] Tasks
        {
            get { return _tasks; }
        }

        public void Add(IProcess process, ITask[] task = null) { }

        public void Remove(IProcess process) { }

        public void Run() { }
    }

    class UndefinedFunction : UndefinedElement, IFunction
    {
        private string[] _tokens = new string[0];

        public string AsmName 
        { 
            get { return ""; }
        }

        public string ModuleName 
        { 
            get { return "";  } 
        }

        public string MethodName
        {
            get { return ""; }
        }

        public System.Reflection.MethodInfo Method
        {
            get { return null; }
        }

        public string FunctionText 
        {
            get { return ""; }
            set { }
        }

        public void SetProperties(string assemblyName, string moduleName, string methodName) { }

        public string[] FunctionTokens
        {
            get { return _tokens; }
        }
    }

    class UndefinedEvent : UndefinedCondition, IEvent
    {
        public IProcess Process
        {
            get { return Undefined.Process; }
            set { }
        }
    }

    class UndefinedProcess : UndefinedElement, IProcess
    {
        private IEvent[] _events = new IEvent[0];
        private ITask[] _tasks = new ITask[0];
        private IValue[] _outputs = new IValue[0];
        private IValueView[] _inputs = new IValueView[0];
        private IContainer[] _containers = new IContainer[0];

        public IEvent StartEvent
        {
            get { return Undefined.Event; }
        }

        public IEvent StopEvent
        {
            get { return Undefined.Event; }
        }

        public IEvent[] Events
        {
            get { return _events; }
            set {}
        }

        public ITask[] Tasks
        {
            get { return _tasks; }
            set { }
        }

        public ITask GetTask(string name)
        {
            return Undefined.Task;
        }

        public ICondition DoneCondition
        {
            get { return Undefined.Condition; }
            set { }
        }

        public IContainer GetContainer(Type taskType)
        {
            return Undefined.Container;
        }

        public void SetContainer(Type taskType, IContainer container) { }

        public IContainer[] Containers
        {
            get { return _containers; }
        }

        public void Run() { }

        public RunState CurrentState
        {
            get { return RunState.Ready; }
        }

        public string FunctionText
        {
            get { return ""; }
            set { }
        }

        public IFunction RunFunction
        {
            get { return Undefined.Function; }
            set { }
        }

        public IValueView[] Inputs
        {
            get { return _inputs; }
            set { }
        }

        public IValueView GetInput(string name)
        {
            return Undefined.View;
        }

        public IValue[] Outputs
        {
            get { return _outputs; }
            set { }
        }

        public IValue GetOutput(string name)
        {
            return Undefined.Value;
        }


        public IEvent GetEvent(string name)
        {
            throw new NotImplementedException();
        }

        public void ResetValues() { }

        public void Cancel() { }

        public IProcess Process
        {
            get { return Undefined.Process; }
            set { }
        }

        public bool Modified 
        {
            get { return false; }
        }
    }

    class UndefinedTask : UndefinedElement, ITask
    {
        static public void DefaultRunFunction(IRunnable task) { }

        public string FunctionText
        {
            get { return ""; }
            set {}
        }

        public void Run() {}

        public RunState CurrentState
        {
            get { return RunState.Done; }
        }

        public IProcess Process
        {
            get { return Undefined.Process; }
            set { }
        }

        public IFunction RunFunction
        {
            get { return Undefined.Function; }
            set { }
        }

        public ICondition StartCondition
        {
            get { return Undefined.Condition; }
            set { }
        }

        public ICondition StopCondition
        {
            get { return Undefined.Condition; }
            set { }
        }

        public IValueView[] Inputs
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IValueView GetInput(string name)
        {
            throw new NotImplementedException();
        }

        public IValue[] Outputs
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IValue GetOutput(string name)
        {
            throw new NotImplementedException();
        }

        public void Cancel() { }

        public bool Modified
        {
            get { return false; }
        }
    }

    class UndefinedValue : UndefinedElement, IValue
    {
        public object Value
        {
            get { return ""; }
            set { }
        }

        public bool Modified
        {
            get { return false; }
        }
    }

    class UndefinedView : UndefinedValue, IValueView
    {

        public IValue SourceValue
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool Required
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
