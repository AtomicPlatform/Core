using System;

namespace Atomic.Core
{
    public class AtomicElement : IElement, IDisposable
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

        public void Dispose()
        {
        }
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

    public class TaskCondition : AtomicCondition
    {
        public TaskCondition()
        {
            Task = null;
            MetFunction = CoreFunctions.DefaultMetFunction;
        }

        public ITask Task { get; set; }

        public override bool Met()
        {
            return MetFunction(Task);
        }

        public Func<ITask, bool> MetFunction { get; set; }
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
            MetFunction = CoreFunctions.DefaultMetFunction;
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

    public class AtomicTask : AtomicElement, ITask
    {
        public AtomicTask()
        {
            StartCondition = new TaskCondition()
            {
                Task = null,
                MetFunction = CoreFunctions.DefaultMetFunction
            };
            StopCondition = new TaskCondition()
            {
                Task = this,
                MetFunction = CoreFunctions.TaskRunCompleted
            };
            RunFunction = CoreFunctions.DefaultRunFunction;

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

        public TaskFunction RunFunction { get; set; }

        public ICondition StartCondition { get; set; }

        public TaskState CurrentState { get; private set; }

        public ICondition StopCondition { get; set; }

        virtual public void Run()
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

    public class AtomicProcess : AtomicElement, IProcess
    {
        public AtomicProcess()
        {
            StartEvent = new AtomicTask()
            {
                StartCondition = null
            };
            StartEvent.StopCondition = new TaskCondition()
            {
                Task = StartEvent,
                MetFunction = CoreFunctions.TaskRunCompleted
            };

            StopEvent = new AtomicTask()
            {
                StartCondition = new TaskCondition()
                {
                    Task = StartEvent,
                    MetFunction = CoreFunctions.TaskDone
                }
            };

            Events = new ITask[] { };
        }

        public ITask StartEvent { get; private set; }

        public ITask[] Events { get; set; }

        public ITask StopEvent { get; private set; }

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

        public void Run()
        {
            switch (CurrentState)
            {
                case TaskState.Ready:
                case TaskState.Active:
                    StartEvent.Run();
                    break;
                case TaskState.Running:
                    foreach (ITask evt in Events)
                    {
                        evt.Run();
                    }
                    break;
                case TaskState.RunComplete:
                    StopEvent.Run();
                    break;
            }
        }

        public TaskState CurrentState
        {
            get;
            private set;
        }

        override public void Update()
        {
            switch (CurrentState)
            {
                case TaskState.Ready:
                    StartEvent.Update();

                    if (StartEvent.CurrentState == TaskState.Done)
                    {
                        CurrentState = TaskState.Running;
                    }
                    else
                    {
                        CurrentState = TaskState.Active;
                    }
                    break;
                case TaskState.Active:
                    StartEvent.Update();

                    if (StartEvent.CurrentState == TaskState.Done)
                    {
                        CurrentState = TaskState.Running;
                    }
                    break;
                case TaskState.Running:
                    foreach (ITask evt in Events)
                    {
                        evt.Update();
                    }

                    if (StopEvent.StartCondition.Met())
                    {
                        CurrentState = TaskState.RunComplete;
                    }
                    break;
                case TaskState.RunComplete:
                    StopEvent.Update();

                    if (StopEvent.CurrentState == TaskState.Done)
                    {
                        CurrentState = TaskState.Done;
                    }
                    else
                    {
                        CurrentState = TaskState.Stopping;
                    }
                    break;
                case TaskState.Stopping:
                    StopEvent.Update();

                    if (StopEvent.CurrentState == TaskState.Done)
                    {
                        CurrentState = TaskState.Done;
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
