using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atomic.Core
{

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
        IValue GetParameter(string name);

        /// <summary>
        /// A list of parameter names held in the message.
        /// </summary>
        string[] ParameterNames { get; }
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

    public class StartEvent : AtomicEvent
    {
        public StartEvent()
        {
            Name = "_start";
        }

        public override bool Met
        {
            get { return Process.CurrentState == RunState.Ready; }
        }
    }

    public class StopEvent : AtomicEvent
    {
        public StopEvent()
        {
            Name = "_stop";
        }

        public override bool Met
        {
            get { return Process.DoneCondition.Met; }
        }
    }

    public class MessageEvent : AtomicEvent, IMessageEvent
    {
        private IMessage _message = null; //Undefined.Message;

        public MessageEvent()
        {
        }

        public IMessage Message
        {
            get { return _message; }
            set { _message = value; }
        }

        public override bool Met
        {
            get { return _message != null; } // Undefined.Message; }
        }
    }
}
