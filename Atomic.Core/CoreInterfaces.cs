namespace Atomic.Core
{
    /// <summary>
    /// A definition for a generic runtime container.  This container will provide 
    /// the proper operating system response to task script definitions.
    /// </summary>
    public interface IContainer
    {
        /// <summary>
        /// A flag to indicate whether the container should allow the process to be debugged.
        /// </summary>
        bool DebugMode { get; set; }

        /// <summary>
        /// The stream used to send debug information back to this container for processing.
        /// </summary>
        System.IO.Stream DebugStream { get; set; }

        /// <summary>
        /// A list of processes held in this container.
        /// </summary>
        ITask[] TaskList { get; }

        /// <summary>
        /// Adds a process task to this container.
        /// </summary>
        /// <param name="task">A task to execute in this container.</param>
        void AddTask(ITask task);

        /// <summary>
        /// Executes all the held processes in the container.
        /// </summary>
        void Run();

        /// <summary>
        /// The operating-specific commands called from a task run script.
        /// </summary>
        /// <param name="task">The task that contains the run script to execute.</param>
        /// <returns>A textual message indicating any issues running the script.</returns>
        string ExecuteTask(ITask task);

        /// <summary>
        /// A container-specific implementation of how to handle error messages.
        /// </summary>
        /// <param name="errorText">The error text generated from the execute function.</param>
        void HandleError(string errorText);
    }

    /// <summary>
    /// The base element/object for all atomic elements.
    /// </summary>
    public interface IElement
    {
        /// <summary>
        /// A system generated identifier based off the element name.
        /// </summary>
        string ID { get; }

        /// <summary>
        /// The name of the element, which can include spaces and other UTF-8 characters.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Updates the state of the element.
        /// </summary>
        void Update();

        /// <summary>
        /// Indicates whether or not changes can be made to the element.
        /// </summary>
        bool Locked { get; set; }

        /// <summary>
        /// Returns if the element is completely defined.
        /// </summary>
        /// <remarks>
        /// An invalid element cannot be locked or used in a running process.
        /// </remarks>
        bool IsValid { get; }
    }

    /// <summary>
    /// An element that represents a static method located in a separate .NET assembly.
    /// <para>
    /// Each element that requires a external function to operate has a unique signature that
    /// must be followed.  Refer to the specific class for the proper method signature.
    /// </para>
    /// </summary>
    public interface IFunction : IElement
    {
        /// <summary>
        /// The full name of the assembly.
        /// </summary>
        /// <example>
        /// Atomic.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
        /// </example>
        string AsmName { get; }

        /// <summary>
        /// The name of module/class holding the static method.
        /// </summary>
        /// <example>
        /// ValueCondition
        /// </example>
        string ModuleName { get; }

        /// <summary>
        /// The name of the static method.
        /// </summary>
        /// <example>
        /// EqualsFunction
        /// </example>
        string MethodName { get; }

        /// <summary>
        /// A callable method definition based on the assembly name, module name, and method name.
        /// </summary>
        System.Reflection.MethodInfo Method { get; }

        /// <summary>
        /// Initializes the function instance with specified assembly, moule, and method names.
        /// </summary>
        /// <param name="assemblyName">The full .NET assembly name.</param>
        /// <param name="moduleName">The name of the module/class within the assembly.</param>
        /// <param name="methodName">The name of the static method within the module/class.</param>
        void SetProperties(string assemblyName, string moduleName, string methodName);
    }

    /// <summary>
    /// An element that stores information used within a process.
    /// </summary>
    public interface IValue : IElement
    {
        /// <summary>
        /// The information stored.
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// Returns whether or not the value has changed during a process tick.
        /// </summary>
        bool Modified { get; }
    }

    public interface IParameter : IValue
    {
        bool InputParameter { get; set; }

        bool Required { get; set; }
    }

    public interface IValueView<T> : IValue
    {
        IValue SourceValue { get; set; }

        new T Value { get; }
    }

    /// <summary>
    /// The base element that can make changes to value elements.
    /// <para>
    /// These elements are stateful so the run and update commands perform specific 
    /// actions based on the element's state.
    /// </para> 
    /// </summary>
    public interface IRunnable : IElement
    {
        /// <summary>
        /// The method used to execute the runnable element.
        /// </summary>
        void Run();

        /// <summary>
        /// The current state of the runnable element.
        /// </summary>
        RunState CurrentState { get; }

        /// <summary>
        /// The function delegate that is executed during the run process.
        /// </summary>
        TaskFunction RunFunction { get; set; }

        /// <summary>
        /// A collection of value elements that are used during the run process. 
        /// </summary>
        IValue[] Values { get; set; }

        /// <summary>
        /// Returns the value contained in the task with the specified name.
        /// </summary>
        /// <param name="name">The case-insensitive name of the value.</param>
        /// <returns>The specified value or <code>Undefined.Value</code> if no value exists.</returns>
        IValue GetValue(string name);
    }

    /// <summary>
    /// A generic interface that indicates whether an element has conditions indicating
    /// when it can start executing and stop executing.
    /// </summary>
    public interface IStartable
    {
        /// <summary>
        /// The condition that must be met to start running an element.
        /// </summary>
        ICondition StartCondition { get; set; }

        /// <summary>
        /// The condition that must be met to stop running an element.
        /// </summary>
        ICondition StopCondition { get; set; }
    }

    /// <summary>
    /// An element that represents an event that takes place on a process.
    /// <para>Note that this is the generic definition which does not include a way
    /// for information to be passed into/send out from the process.</para>
    /// </summary>
    public interface IEvent : IElement
    {
        /// <summary>
        /// The process containing the event.
        /// </summary>
        IProcess Process { get; set; }

        /// <summary>
        /// The condition that must be met before the event is triggered.
        /// </summary>
        ICondition StartCondition { get; }
    }

    /// <summary>
    /// An event element that allows for value elements to be brought into or sent out
    /// from the process.
    /// </summary>
    public interface IMessageEvent : IEvent
    {
        /// <summary>
        /// The message associated with the event.
        /// </summary>
        IMessage Message { get; }
    }

    /// <summary>
    /// An element that contains a task as a script that run while the task is running. 
    /// </summary>
    public interface ITask : IRunnable, IStartable
    {
        /// <summary>
        /// The function script that is to be run.  The container where this task 
        /// resides determines how to process the script.
        /// </summary>
        string FunctionText { get; set; }

        /// <summary>
        /// Returns the value containing the result from running the function script.
        /// </summary>
        IValue RunResult { get; }
    }

    /// <summary>
    /// An element that provides either a start or stop condition to a runnable element.
    /// </summary>
    public interface ICondition : IElement
    {
        /// <summary>
        /// Returns if the condition has been met.
        /// </summary>
        bool Met { get; }
    }

    /// <summary>
    /// A task element that contains a group of related tasks as an activity.
    /// </summary>
    /// <remarks>An activity allows for encapsulating tasks, preventing other tasks from
    /// accessing internal task states and values.  To make such information known, these
    /// values can be linked to the activity value collection.</remarks>
    public interface IActivity : ITask
    {
        /// <summary>
        /// A collection of tasks in the activity.
        /// </summary>
        ITask[] Tasks { get; set; }
    }

    /// <summary>
    /// An element that represents a runnable process that is added to a run container.
    /// </summary>
    /// <remarks>
    /// The start event is triggered when the process is added to the run container.  The process
    /// is removed from the run container when the state of the process is "Done".
    /// </remarks>
    public interface IProcess : IRunnable
    {
        /// <summary>
        /// An event that is triggered when the process is added to a run container.
        /// </summary>
        IEvent StartEvent { get; }

        /// <summary>
        /// An event that is triggered when the process has completed.
        /// </summary>
        IEvent StopEvent { get; }

        /// <summary>
        /// A collection of other events that are accessible while the process is running.
        /// </summary>
        IEvent[] Events { get; set; }

        /// <summary>
        /// A collection of tasks that can run while the process is running.
        /// </summary>
        ITask[] Tasks { get; set; }

        /// <summary>
        /// A condition that must be met for the process to complete.
        /// </summary>
        ICondition DoneCondition { get; set; }
    }

    /// <summary>
    /// An element that holds a collection of parameters used to transport information
    /// between processes.
    /// </summary>
    public interface IMessage : IElement
    {
        /// <summary>
        /// Gets the value element associated with the specific name.
        /// </summary>
        /// <param name="name">The case-insensitive name of a parameter in the message.</param>
        /// <returns>An event parameter structure holding the parameter value and type.</returns>
        IParameter GetParameter(string name);

        /// <summary>
        /// A list of parameter names held in the message.
        /// </summary>
        string[] ParameterNames { get; }
    }

    /// <summary>
    /// An enumeration of states for a runnable element.
    /// </summary>
    public enum RunState
    {
        /// <summary>The default state.  The start condition is not met.</summary>
        Ready,
        /// <summary>The element is active but not ready for running.  The start condition was met. Applies to tasks only.</summary>
        Active,
        /// <summary>The element is currently running. The stop condition is not met.</summary>
        Running,
        /// <summary>The element has completed running.  The stop condition was met.</summary>
        RunComplete,
        /// <summary>The element is performing final operations prior to being done.</summary>
        Stopping,
        /// <summary>The element has completed.</summary>
        Done
    }

    public interface IDataAccessContainer : IContainer
    {
        void Read(IMessage message);

        void Write(IMessage message);
    }

    public interface IDataDefinitionContainer : IContainer
    {
        void Create(IMessage message);

        void Modify(IMessage message);

        void Delete(IMessage message);

        void Move(IMessage message);

        void Copy(IMessage message);

        void Open(IMessage message);

        void Close(IMessage message);

        void List(IMessage message);
    }

    public interface IDataManipulationContainer : IContainer
    {
        IValue[] Select(IMessage message);

        void Update(IMessage message);

        void Delete(IMessage message);
    }

    /// <summary>
    /// The delegate signature for task functions.
    /// </summary>
    /// <param name="task"></param>
    public delegate void TaskFunction(IRunnable task);
}
