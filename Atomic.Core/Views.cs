using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atomic.Core
{

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
            get { return Condition.Met; }
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
}
