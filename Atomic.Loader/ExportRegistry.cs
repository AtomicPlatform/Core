using Atomic.Core;
using System.Collections.Generic;
using System.Linq;

namespace Atomic.Loader
{
    public class ExportRegistry
    {
        private IDictionary<string, IEvent> _events = new Dictionary<string, IEvent>();
        private IDictionary<string, ITask> _tasks = new Dictionary<string, ITask>();
        private IDictionary<string, ICondition> _conditions = new Dictionary<string, ICondition>();
        private IDictionary<string, IValue> _values = new Dictionary<string, IValue>();
        private IDictionary<string, IFunction> _functions = new Dictionary<string, IFunction>();
        private IDictionary<string, IContainer> _containers = new Dictionary<string, IContainer>();

        private IDictionary<string, IProcess> _process = new Dictionary<string, IProcess>();

        public ExportRegistry() { }

        /// <summary>
        /// Creates a collection of core elements based on the definitions contained in 
        /// the specified process model.
        /// </summary>
        /// <param name="model">
        ///     The process model containing all the element models
        ///     needed for a process element.
        /// </param>
        public ExportRegistry(IProcessModel model)
        {
            _process[model.ID] = new AtomicProcess();
            IEvent evt = null;

            foreach (EventModel evtModel in model.Events)
            {
                switch (evtModel.EventType) {
                    case EventModel.StartEventType:
                        evt = new StartEvent();
                        break;
                    case EventModel.StopEventType:
                        evt = new StopEvent();
                        break;
                    default:
                        break;
                }

                if (evt != null) _events.Add(evtModel.ID, evt);
            }

            // a distinction needs to be made in the model
            foreach (TaskModel taskModel in model.Tasks)
            {
                switch (taskModel.TaskType)
                {
                    default:
                    case TaskModel.GeneralTask:
                        _tasks.Add(taskModel.ID, new GeneralTask());
                        break;
                }
            }

            foreach (ConditionModel condModel in model.Conditions)
            {
                _conditions.Add(condModel.ID, new TaskCondition());
            }

            foreach (ValueModel valModel in model.Values)
            {
                _values.Add(valModel.ID, new AtomicValue());
            }

            foreach (FunctionModel funcModel in model.Functions)
            {
                _functions.Add(funcModel.ID, new AtomicFunction());
            }
        }

        /// <summary>
        /// The process element contained in the registry.
        /// </summary>
        public IProcess Process
        {
            get { return _process.Values.First(); }
        }

        /// <summary>
        /// Returns the event element with the corresponding identifier.
        /// </summary>
        /// <param name="id">The identifier of the event element.</param>
        /// <returns>The event element with the specified identifier.</returns>
        public IEvent GetEvent(string id)
        {
            return null;
        }

        /// <summary>
        /// Returns the task element with the corresponding identifier.
        /// </summary>
        /// <param name="id">The identifier of the task element.</param>
        /// <returns>The task element with the specified identifier.</returns>
        public IRunnable GetTask(string id)
        {
            if (_process.ContainsKey(id))
            {
                return Process;
            }

            return null;
        }

        /// <summary>
        /// Returns the condition element with the corresponding identifier.
        /// </summary>
        /// <param name="id">The identifier of the condition element.</param>
        /// <returns>The condition element with the specified identifier.</returns>
        public ICondition GetCondition(string id)
        {
            return _conditions[id];
        }

        /// <summary>
        /// Returns the value element with the corresponding identifier.
        /// </summary>
        /// <param name="id">The identifier of the value element.</param>
        /// <returns>The value element with the specified identifier.</returns>
        public IValue GetValue(string id)
        {
            return _values[id];
        }

        /// <summary>
        /// Returns the function element with the corresponding identifier.
        /// </summary>
        /// <param name="id">The identifier of the function element.</param>
        /// <returns>The function element with the specified identifier.</returns>
        public IFunction GetFunction(string id)
        {
            return _functions[id];
        }

        public IContainer GetContainer(string id)
        {
            return _containers[id];
        }
    }
}
