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
        static private IMessage _message = new UndefinedMessage();
        static private IParameter _param = new UndefinedParameter();

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

        static public IMessage Message
        {
            get { return _message; }
        }

        static public IParameter Parameter
        {
            get { return _param; }
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

    class UndefinedMessage : UndefinedElement, IMessage
    {
        private string[] _params = new string[0];

        public IParameter GetParameter(string name)
        {
            return Undefined.Parameter;
        }

        public string[] ParameterNames
        {
            get { return _params; }
        }
    }

    class UndefinedParameter : UndefinedValue, IParameter
    {

        public bool InputParameter
        {
            get { return true; }
            set {}
        }

        public bool Required
        {
            get { return false; }
            set {}
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

        public IValue GetValue(string name)
        {
            return Undefined.Value;
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

        public IValue RunResult
        {
            get { return Undefined.Value; }
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
