
namespace Atomic.Core
{
    public interface IContainer
    {
        bool DebugMode { get; set; }

        System.IO.Stream DebugStream { get; set; }

        IProcess[] ProcessList { get; }

        void AddProcess(IProcess p);

        void Run();

        void ExecuteFunction(string functionText);
    }

    public interface IElement
    {
        string ID { get; }

        string Name { get; set; }

        void Update();

        bool Locked { get; set; }
    }

    public interface IFunction : IElement
    {
        string AsmName { get; }

        string ModuleName { get; }

        string MethodName { get; }

        System.Reflection.MethodInfo Method { get; }

        void SetProperties(string assemblyName, string moduleName, string methodName);
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

        IEvent DefaultStartEvent { get; }

        IEvent DefaultStopEvent { get; }
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
