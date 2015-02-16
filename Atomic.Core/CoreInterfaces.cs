
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
    }

    public interface ITask : IRunnable
    {
        IValue[] Values { get; set; }

        TaskFunction RunFunction { get; set; }

        ICondition StartCondition { get; set; }

        ICondition StopCondition { get; set; }
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
        ITask StartEvent { get; }

        ITask[] Events { get; set; }

        ITask StopEvent { get; }
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

    public delegate void TaskFunction(ITask task);
}
