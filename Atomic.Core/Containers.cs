using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Atomic.Core
{
    public interface IPresentationContainer
    {
        TaskFunction ReadFunction { get; }

        TaskFunction WriteFunction { get; }
    }

    abstract public class PresentationContainer : AtomicContainer, IPresentationContainer
    {
        public TaskFunction ReadFunction
        {
            get;
            protected set;
        }

        public TaskFunction WriteFunction
        {
            get;
            protected set;
        }

        private bool IsWriteCommand(string functionText)
        {
            return functionText.ToUpper().StartsWith("WRITE ");
        }

        private bool IsReadCommand(string functionText)
        {
            return functionText.ToUpper().StartsWith("READ ");
        }

        protected override MethodInfo GenerateMethod(IFunction func)
        {
            TaskFunction funcMethod = null;

            if (IsReadCommand(func.FunctionText))
            {
                funcMethod = ReadFunction;
            }
            else if (IsWriteCommand(func.FunctionText))
            {
                funcMethod = WriteFunction;
            }

            return (funcMethod == null) ? null : funcMethod.GetMethodInfo();
        }
    }

    abstract public class DataContainer : AtomicContainer
    {

    }

    abstract public class FileAccessContainer : AtomicContainer
    {

    }

    abstract public class FileSystemContainer : AtomicContainer
    {

    }

    abstract public class NetworkContainer : AtomicContainer
    {

    }

    abstract public class UniversalContainer : AtomicContainer
    {
        private ISet<IProcess> _processSet = new HashSet<IProcess>();
        private ISet<ITask> _taskSet = new HashSet<ITask>();

        public TaskFunction ProcessRunFunction
        {
            get;
            protected set;
        }

        public TaskFunction TaskRunFunction 
        { 
            get; 
            protected set; 
        }

        protected override System.Reflection.MethodInfo GenerateMethod(IFunction func)
        {
            return TaskRunFunction.GetMethodInfo();
        }

        protected override System.Reflection.MethodInfo GenerateMethod(IProcess process)
        {
            return ProcessRunFunction.GetMethodInfo();
        }

        public void AddWatch(IProcess process)
        {
            _processSet.Add(process);
        }

        public void AddWatch(ITask task)
        {
            _taskSet.Add(task);
        }

        protected override IProcess[] GetRunnableProcesses()
        {
            return _processSet.ToArray();
        }

        protected override ITask[] GetRunnableTasks()
        {
            return _taskSet.ToArray();
        }
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

    /*
    public class BasicContainer : AtomicContainer
    {
        protected override MethodInfo GenerateMethod(IFunction func)
        {
            return func.Method;
        }
    }
    */

    /// <summary>
    /// The delegate signature for task functions.
    /// </summary>
    /// <param name="task"></param>
    public delegate void TaskFunction(IRunnable task);

    /// <summary>
    /// The delegate signature for task functions.
    /// </summary>
    /// <param name="task"></param>
    public delegate bool ConditionFunction(ICondition condition);
}
