using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atomic.Core
{
    public class StartEvent : AtomicEvent
    {
        public StartEvent()
        {
            Name = "_start";
        }

        protected override void InitializeStartCondition()
        {
            StartCondition = new ValueCondition()
            {
                Value = new TaskStateView() { Task = Process },
                ExpectedValue = new AtomicValue() { Value = TaskState.Ready }
            };
        }
    }

    public class StopEvent : AtomicEvent
    {
        public StopEvent()
        {
            Name = "_stop";
        }

        protected override void InitializeStartCondition()
        {
            StartCondition = Process.DoneCondition;
        }

        internal void UpdateCondition(ICondition cond)
        {
            StartCondition = cond;
        }
    }
}
