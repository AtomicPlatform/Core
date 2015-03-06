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

        static public ICondition Condition
        {
            get { return _condition; }
        }

        static public IEvent Event
        {
            get { return _event; }
        }

        static public IFunction Function
        {
            get { return _function; }
        }

        static public ITask Task
        {
            get { return _task; }
        }

        static public IValue Value
        {
            get { return _value; }
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
    }

    class UndefinedCondition : UndefinedElement, ICondition 
    {
        public bool Met 
        { 
            get { return false; } 
        }
    }

    class UndefinedFunction : UndefinedElement, IFunction
    {
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

        public void SetProperties(string assemblyName, string moduleName, string methodName) { }
    }

    class UndefinedEvent : UndefinedElement, IEvent
    {
        public ICondition StartCondition
        {
            get { return Undefined.Condition; }
            set { }
        }

        public IProcess Process
        {
            get { return null; }
            set { }
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

        public TaskFunction RunFunction
        {
            get { return UndefinedTask.DefaultRunFunction; }
            set { }
        }

        public IValue[] Values
        {
            get { return new IValue[0]; }
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
}
