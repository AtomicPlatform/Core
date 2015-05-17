using System;

namespace Atomic.Core
{

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

        /// <summary>
        /// The text script associated with the function.
        /// </summary>
        string FunctionText { get; set; }

        /// <summary>
        /// The text script in tokenized form.
        /// </summary>
        string[] FunctionTokens { get; }
    }

    /// <summary>
    /// An element that stores information used within a task.  The base value element is used
    /// as an output to a task.
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

    /// <summary>
    /// Represents an abstract view of a source value element.  The view element is used as an
    /// input to a task.
    /// </summary>
    public interface IValueView : IValue
    {
        /// <summary>
        /// The source value of the view.  The data of the source value is read as needed.
        /// </summary>
        IValue SourceValue { get; set; }

        /// <summary>
        /// The resulting data of the source value's data after conversion by the view.
        /// </summary>
        new object Value { get; }

        /// <summary>
        /// Indicates whether or not this view is required by the enclosing task as an input.
        /// </summary>
        bool Required { get; set; }
    }

    /// <summary>
    /// A view element that converts the data of the source value into data of type <code>T</code>.
    /// </summary>
    /// <typeparam name="T">Any object-based class, which includes primitives like <code>Int32</code>
    /// </typeparam>
    public interface IValueView<T> : IValueView
    {
        /// <summary>
        /// Returns the data contained in the source value as data of type <code>T</code>.
        /// </summary>
        T ViewValue { get; }
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
        /// The process associated with the runnable element.
        /// </summary>
        IProcess Process { get; set; }

        /// <summary>
        /// The actual function that is executed during the run process.  
        /// </summary>
        IFunction RunFunction { get; set; }

        /// <summary>
        /// A collection of input view elements that are used during the run process. 
        /// </summary>
        IValueView[] Inputs { get; set; }

        /// <summary>
        /// Returns the input view contained in the task with the specified name.
        /// </summary>
        /// <param name="name">The case-insensitive name of the input view.</param>
        /// <returns>The specified view or <code>Undefined.View</code> if no input exists.</returns>
        IValueView GetInput(string name);

        /// <summary>
        /// A collection of output value elements that are populated during the run process. 
        /// </summary>
        /// <remarks>
        /// If more than one task will be modifying an output value, the value should be physically 
        /// located at the parent level (like the process), and a pointer to that value element is 
        /// placed in the task.
        /// </remarks>
        IValue[] Outputs { get; set; }

        /// <summary>
        /// Returns the output value contained in the task with the specified name.
        /// </summary>
        /// <param name="name">The case-insensitive name of the value.</param>
        /// <returns>The specified value or <code>Undefined.Value</code> if no value exists.</returns>
        IValue GetOutput(string name);

        /// <summary>
        /// Cancels a running task.
        /// </summary>
        void Cancel();

        /// <summary>
        /// Returns if the runnable has changed states.
        /// </summary>
        bool Modified { get; }
    }

    /// <summary>
    /// An element that represents an event that takes place on a process.
    /// </summary>
    /// <remarksNote that this is the generic definition which does not include a way
    /// for information to be passed into/send out from the process.</remarks>
    public interface IEvent : ICondition
    {
        /// <summary>
        /// The process containing the event.
        /// </summary>
        IProcess Process { get; set; }
    }

    /// <summary>
    /// An element that contains a task as a script that run while the task is running. 
    /// </summary>
    public interface ITask : IRunnable
    {
        /// <summary>
        /// The condition that must be met to start running the task element.
        /// </summary>
        ICondition StartCondition { get; set; }

        /// <summary>
        /// The condition that must be met to stop running the task element.
        /// </summary>
        ICondition StopCondition { get; set; }
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
        /// Returns the event with the specified case-insenstivie name.
        /// </summary>
        /// <param name="name">The name of the event to return.</param>
        /// <returns>The event with the specified name.</returns>
        IEvent GetEvent(string name);

        /// <summary>
        /// A collection of tasks that can run while the process is running.
        /// </summary>
        ITask[] Tasks { get; set; }

        /// <summary>
        /// Returns the task with the specified case-insensitive name.
        /// </summary>
        /// <param name="name">The name of the task to return.</param>
        /// <returns>
        /// The task with the specified name, or <code>Undefined.Task</code> 
        /// if it does not exist.
        /// </returns>
        ITask GetTask(string name);

        /// <summary>
        /// A condition that must be met for the process to complete.
        /// </summary>
        ICondition DoneCondition { get; set; }

        /// <summary>
        /// Resets the modified flag of output values
        /// </summary>
        void ResetValues();
    }

    /// <summary>
    /// An enumeration of states for a runnable element.
    /// </summary>
    public enum RunState
    {
        /// <summary>The default state.  The start condition is not met.</summary>
        Ready,
        /// <summary>The element is active but not ready for running.  The start condition was met. Applies to tasks only.</summary>
        Starting,
        /// <summary>The element is currently running. The stop condition is not met.</summary>
        Running,
        /// <summary>The element has completed running.  The stop condition was met.</summary>
        RunComplete,
        /// <summary>The element is performing final operations prior to being done.</summary>
        Stopping,
        /// <summary>The element has completed.</summary>
        Done
    }


    /// <summary>
    /// A definition for a generic runtime container.  This container will provide 
    /// the proper operating system response to task script definitions.
    /// </summary>
    public interface IContainer : IElement
    {
        /// <summary>
        /// A list of processes held in this container.
        /// </summary>
        ITask[] Tasks { get; }

        /// <summary>
        /// Adds a process to this container.
        /// </summary>
        /// <param name="process">A process containing tasks to execute in this container.</param>
        void Add(IProcess process, ITask[] tasks = null);

        /// <summary>
        /// Removes a process from this container.
        /// </summary>
        /// <param name="process">A process containing tasks to execute in this container.</param>
        void Remove(IProcess process);

        /// <summary>
        /// Executes all the held processes in the container.
        /// </summary>
        void Run();
    }
}
