﻿using System;
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
        private IDictionary<string, IRunnable> _tasks = new Dictionary<string, IRunnable>();
        private IDictionary<string, ICondition> _conditions = new Dictionary<string, ICondition>();
        private IDictionary<string, IValue> _values = new Dictionary<string, IValue>();

        // improve this to evaluate the model to create the correct object type
        public ExportRegistry(IProcessModel model)
        {
            foreach (EventModel evtModel in model.Events)
            {
                _events.Add(evtModel.ID, new AtomicEvent());
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
        }

        public IEvent GetEvent(string id)
        {
            return null;
        }

        public IRunnable GetTask(string id)
        {
            return null;
        }

        public ICondition GetCondition(string id)
        {
            return null;
        }

        public IValue GetValue(string id)
        {
            return null;
        }
    }
}