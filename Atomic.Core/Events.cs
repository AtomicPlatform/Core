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
                ExpectedValue = new AtomicValue() { Value = RunState.Ready }
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

    public class MessageEvent : AtomicEvent, IMessageEvent
    {
        private IMessage _message = Undefined.Message;

        public MessageEvent()
        {
        }

        public IMessage Message
        {
            get { return _message; }
            set { _message = value; }
        }

        protected override void InitializeStartCondition()
        {
            StartCondition = new ValueCondition()
            {
                Name = "Message Start Condition",
                Value = new AtomicValue() { Value = Message },
                ExpectedValue = new AtomicValue() { Value = Undefined.Message },
                MetFunction = ValueCondition.NotEqualsFunction
            };
        }
    }
}
