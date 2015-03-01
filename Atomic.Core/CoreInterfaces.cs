
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

        IValue[] Values { get; set; }
    }

    public interface IStartable
    {
        ICondition StartCondition { get; set; }

        ICondition StopCondition { get; set; }
    }

    public interface IEvent : IElement, IStartable
    {
        IValue[] Values { get; set; }
    }

    public interface ITask : IRunnable, IStartable
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

    public interface IMessage : IElement
    {
        IValue[] Values { get; set; }
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
