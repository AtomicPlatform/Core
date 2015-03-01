using System;

namespace Atomic.Core
{
    public class AtomicElement : IElement
    {
        public AtomicElement()
        {
            Name = ElementName + GetHashCode();
        }

        virtual public bool Locked { get; set; }

        virtual public void Update() { }

        public string Name { get; set; }

        virtual protected string ElementName
        {
            get { return "element"; }
        }
    }

    public class AtomicMessage : AtomicElement, IMessage
    {
        public IValue[] Values { get; set; }
    }

    public class AtomicValue : AtomicElement, IValue
    {
        private object _publicValue = null;
        private object _internalValue = null;

        public bool Modified { get; private set; }

        public object Value
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
    }

    abstract public class AtomicCondition : AtomicElement, ICondition
    {
        abstract public bool Met();

        protected override string ElementName
        {
            get { return "condition"; }
        }
    }

    public class StandaloneCondition : AtomicCondition
    {
        public override bool Met()
        {
            return MetFunction();
        }

        public Func<bool> MetFunction { get; set; }
    }

    public class TaskCondition : AtomicCondition
    {
        public TaskCondition()
        {
            Task = null;
            MetFunction = TestStateFunction;
        }

        public IRunnable Task { get; set; }

        public TaskState State { get; set; }

        public override bool Met()
        {
            return MetFunction(Task);
        }

        private bool TestStateFunction(IRunnable task)
        {
            return Task.CurrentState == State;
        }

        public Func<IRunnable, bool> MetFunction { get; set; }
    }

    public class ValueCondition : AtomicCondition
    {
        public ValueCondition()
        {
            Value = new AtomicValue();
            MetFunction = CoreFunctions.DefaultMetFunction;
        }

        public IValue Value { get; set; }

        public override bool Met()
        {
            return MetFunction(Value);
        }

        public Func<IValue, bool> MetFunction { get; set; }
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
        static public ICondition DefaultCondition = new StandaloneCondition()
        {
            MetFunction = CoreFunctions.DefaultMetFunction
        };

        public AtomicEvent()
        {
            StartCondition = DefaultCondition;
            StopCondition = DefaultCondition;

            Values = new IValue[0];
        }

        public IValue[] Values { get; set; }

        override public bool Locked
        {
            get { return base.Locked; }
            set
            {
                foreach (IValue v in Values)
                {
                    v.Locked = value;
                }

                if (StartCondition != null) StartCondition.Locked = value;
                if (StopCondition != null) StopCondition.Locked = value;

                base.Locked = value;
            }
        }

        public ICondition StartCondition { get; set; }

        public ICondition StopCondition { get; set; }

        protected override string ElementName
        {
            get { return "event"; }
        }
    }

    abstract public class AtomicRunnable : AtomicElement, IRunnable
    {
        public IValue[] Values { get; set; }

        public TaskFunction RunFunction { get; set; }

        public TaskState CurrentState { get; set; }

        abstract public void Run();
    }

    public class AtomicTask : AtomicRunnable, ITask
    {
        public ICondition StartCondition { get; set; }

        public ICondition StopCondition { get; set; }

        public AtomicTask()
        {
            StopCondition = new TaskCondition()
            {
                Task = this,
                State = TaskState.RunComplete
            };

            StartCondition = new StandaloneCondition()
            {
                MetFunction = CoreFunctions.DefaultMetFunction
            };

            RunFunction = CoreFunctions.DefaultRunFunction;
        }

        public string FunctionText { get; set; }

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
        public AtomicActivity()
        {
            Tasks = new ITask[0];
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
            get;
            set;
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
        public AtomicProcess()
        {
            StartEvent = new AtomicEvent()
            {
                Name = "_start",
                StartCondition = new TaskCondition()
                {
                    Task = this,
                    State = TaskState.Ready
                },
                StopCondition = new TaskCondition()
                {
                    Task = this,
                    State = TaskState.Active
                }
            };

            StopEvent = new AtomicEvent()
            {
                Name = "_stop",
                StartCondition = new TaskCondition()
                {
                    Task = this,
                    State = TaskState.Running
                },
                StopCondition = new TaskCondition()
                {
                    Task = this,
                    State = TaskState.RunComplete
                }
            };

            Events = new IEvent[] { };
            Tasks = new ITask[] { };
            Values = new IValue[] { };
        }

        public IEvent StartEvent { get; private set; }

        public IEvent[] Events { get; set; }

        public IEvent StopEvent { get; private set; }

        public ITask[] Tasks { get; set; }

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
            switch (CurrentState)
            {
                case TaskState.Ready:
                case TaskState.Active:
                    break;
                case TaskState.Running:
                    foreach (ITask task in Tasks)
                    {
                        task.Run();
                    }
                    break;
                case TaskState.RunComplete:
                    break;
            }
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
