using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace Atomic.Core
{

    public class StandaloneCondition : AtomicCondition
    {
        static public bool DefaultFunction() { return false; }

        public StandaloneCondition()
        {
            Func<bool> func = StandaloneCondition.DefaultFunction;
            FunctionElement = new AtomicFunction(func.GetMethodInfo());
        }

        public override bool Met
        {
            get { return MetFunction(); }
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
        private RunState _state = RunState.Done;

        public TaskCondition()
        {
            Func<TaskCondition, bool> func = TaskCondition.DefaultFunction;
            FunctionElement = new AtomicFunction(func.GetMethodInfo());
        }

        public override bool Met
        {
            get { return MetFunction(this); }
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

        public RunState State
        {
            get { return _state; }
            set { _state = value; }
        }
    }

    public class ValueCondition : AtomicCondition
    {
        static public bool EqualsFunction(ValueCondition valCond)
        {
            return valCond.Value.Equals(valCond.ExpectedValue);
        }

        static public bool NotEqualsFunction(ValueCondition valCond)
        {
            return !EqualsFunction(valCond);
        }

        private IValue _value = Undefined.Value;
        private IValue _expected = Undefined.Value;

        public ValueCondition()
        {
            Func<ValueCondition, bool> func = ValueCondition.EqualsFunction;
            FunctionElement = new AtomicFunction(func.GetMethodInfo());
        }

        public override bool Met
        {
            get { return MetFunction(this); }
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
        static public bool AllConditionsMet(ICondition[] conditions)
        {
            bool met = true;

            foreach (ICondition c in conditions)
            {
                met &= c.Met;
            }

            return met;
        }

        static public bool AnyConditionMet(ICondition[] conditions)
        {
            bool met = false;
            if (conditions.Length == 0) return true;

            foreach (ICondition c in conditions)
            {
                met |= c.Met;
            }

            return met;
        }

        public RuleCondition()
        {
            Conditions = new ICondition[] { };
            MetFunction = RuleCondition.AllConditionsMet;
        }

        public ICondition[] Conditions { get; set; }

        public Func<ICondition[], bool> MetFunction { get; set; }

        public override bool Met
        {
            get { return MetFunction(Conditions); }
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

        public override bool Met
        {
            get
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
    }
}
