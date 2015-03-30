using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Atomic.Core
{

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

    public class BasicContainer : AtomicContainer
    {
        protected override MethodInfo GenerateMethod(IFunction func)
        {
            return func.Method;
        }
    }

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
