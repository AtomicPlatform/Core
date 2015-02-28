
namespace Atomic.Core
{
    public interface IElement
    {
        string Name { get; set; }

        void Update();

        bool Locked { get; set; }
    }

    public interface IValue : IElement
    {
        object Value { get; set; }

        bool Modified { get; }
    }

    public interface IRunnable : IElement
    {
        void Run();

        TaskState CurrentState { get; }

        TaskFunction RunFunction { get; set; }

        ICondition StartCondition { get; set; }

        ICondition StopCondition { get; set; }

        IValue[] Values { get; set; }
    }

    public interface IEvent : IElement
    {
        IValue[] Values { get; set; }

        ICondition Condition { get; set; }
    }

    public interface ITask : IRunnable
    {
        string FunctionText { get; set; }
    }

    public interface ICondition : IElement
    {
        bool Met();
    }

    public interface IActivity : ITask
    {
        ITask[] Tasks { get; set; }
    }

    public interface IProcess : IRunnable
    {
        IEvent StartEvent { get; }

        IEvent StopEvent { get; }

        IEvent[] Events { get; set; }

        ITask[] Tasks { get; set; }
    }

    public enum TaskState
    {
        Ready,
        Active,
        Running,
        RunComplete,
        Stopping,
        Done
    }

    public delegate bool ConditionFunction(ITask task);

    public delegate void TaskFunction(IRunnable task);
}
