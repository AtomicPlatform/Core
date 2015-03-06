using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Atomic.Core;

namespace Atomic.Loader
{
    public class ExportRegistry
    {
        private IDictionary<string, IEvent> _events = new Dictionary<string, IEvent>();
        private IDictionary<string, ITask> _tasks = new Dictionary<string, ITask>();
        private IDictionary<string, ICondition> _conditions = new Dictionary<string, ICondition>();
        private IDictionary<string, IValue> _values = new Dictionary<string, IValue>();
        private IDictionary<string, IFunction> _functions = new Dictionary<string, IFunction>();

        private IDictionary<string, IProcess> _process = new Dictionary<string, IProcess>();

        // improve this to evaluate the model to create the correct object type
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

            foreach (TaskModel taskModel in model.Tasks)
            {
                _tasks.Add(taskModel.ID, new AtomicTask());
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
                _values.Add(funcModel.ID, new AtomicValue());
            }
        }

        public IProcess Process
        {
            get { return _process.Values.First(); }
        }

        public IEvent GetEvent(string id)
        {
            return null;
        }

        public IRunnable GetTask(string id)
        {
            if (_process.ContainsKey(id))
            {
                return Process;
            }

            return null;
        }

        public ICondition GetCondition(string id)
        {
            return _conditions[id];
        }

        public IValue GetValue(string id)
        {
            return _values[id];
        }

        public IFunction GetFunction(string id)
        {
            return _functions[id];
        }

    }
}
