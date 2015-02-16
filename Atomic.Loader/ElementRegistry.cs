using System;
using System.Collections.Generic;

using Atomic.Core;
using System.Reflection;

namespace Atomic.Loader
{

    public class ElementRegistry
    {
        public ElementRegistry()
        {
            Conditions = new Dictionary<string, RegistryEntry>();
            Functions = new Dictionary<string, RegistryEntry>();
            Tasks = new Dictionary<string, RegistryEntry>();
            Values = new Dictionary<string, RegistryEntry>();
        }

        public RegistryEntry Process { get; set; }

        public IDictionary<string, RegistryEntry> Conditions
        {
            get;
            private set;
        }

        public IDictionary<string, RegistryEntry> Tasks
        {
            get;
            private set;
        }

        public IDictionary<string, RegistryEntry> Values
        {
            get;
            private set;
        }

        public IDictionary<string, RegistryEntry> Functions
        {
            get;
            private set;
        }

        private void ClearRegistry()
        {
            Process = null;
            Conditions.Clear();
            Functions.Clear();
            Tasks.Clear();
            Values.Clear();
        }

        public IDictionary<string, object> Export()
        {
            IDictionary<string, object> exportData = new Dictionary<string, object>();
            IDictionary<string, string> process = new Dictionary<string, string>();

            // export process
            foreach (KeyValuePair<string, string> keyPair in Process.Attributes)
            {
                process[keyPair.Key] = keyPair.Value;
            }
            exportData["process"] = process;

            List<IDictionary<string, string>> conditionList = new List<IDictionary<string, string>>();
            foreach (RegistryEntry conditionEntry in Conditions.Values)
            {
                conditionList.Add(conditionEntry.Attributes);
            }
            exportData["conditions"] = conditionList;

            List<IDictionary<string, string>> taskList = new List<IDictionary<string, string>>();
            foreach (RegistryEntry taskEntry in Tasks.Values)
            {
                taskList.Add(taskEntry.Attributes);
            }
            exportData["tasks"] = taskList;

            List<IDictionary<string, string>> valueList = new List<IDictionary<string, string>>();
            foreach (RegistryEntry valueEntry in Values.Values)
            {
                valueList.Add(valueEntry.Attributes);
            }
            exportData["values"] = valueList;

            List<IDictionary<string, string>> functionList = new List<IDictionary<string, string>>();
            foreach (RegistryEntry functionEntry in Functions.Values)
            {
                functionList.Add(functionEntry.Attributes);
            }
            exportData["functions"] = functionList;

            return exportData;
        }


        public void Import(IDictionary<string, object> importData)
        {
            ClearRegistry();

            IDictionary<string, string> process = (IDictionary<string, string>)importData["process"];
            List<IDictionary<string, string>> conditions = (List<IDictionary<string, string>>)importData["conditions"];
            List<IDictionary<string, string>> tasks = (List<IDictionary<string, string>>)importData["tasks"];
            List<IDictionary<string, string>> values = (List<IDictionary<string, string>>)importData["values"];
            List<IDictionary<string, string>> functions = (List<IDictionary<string, string>>)importData["functions"];

            Process = RegistryEntry.CreateEntry(process);
            RegistryEntry entry = null;
            foreach (IDictionary<string, string> condition in conditions)
            {
                entry = RegistryEntry.CreateEntry(condition);
                Conditions[entry.ID] = entry;
            }

            foreach (IDictionary<string, string> task in tasks)
            {
                entry = RegistryEntry.CreateEntry(task);
                Tasks[entry.ID] = entry;
            }

            foreach (IDictionary<string, string> value in values)
            {
                entry = RegistryEntry.CreateEntry(value);
                Values[entry.ID] = entry;
            }

            foreach (IDictionary<string, string> function in functions)
            {
                entry = RegistryEntry.CreateEntry(function);
                Functions[entry.ID] = entry;
            }
        }

        public void Import(IProcess process)
        {
            ClearRegistry();

            if (process == null) return;

            RegistryEntry entry = RegistryEntry.CreateEntry(process);
            Process = entry;

            // loop through each element contained in the process, saving to the appropriate
            // collection
            ImportTask(process.StartEvent);
            ImportTask(process.StopEvent);

            foreach (ITask evt in process.Events)
            {
                ImportTask(evt);
            }
        }

        private void ImportCondition(ICondition condition)
        {
            if (condition == null) return;
            if (Conditions.ContainsKey(condition.GetHashCode().ToString())) return;

            RegistryEntry entry = RegistryEntry.CreateEntry(condition);
            Conditions.Add(entry.ID, entry);

            // add task, value, or conditions entries as needed
            if (condition is TaskCondition)
            {
                TaskCondition tc = (TaskCondition)condition;
                ImportTask(tc.Task);
                ImportFunction(tc.MetFunction);
            }
            else if (condition is RuleCondition)
            {
                RuleCondition rc = (RuleCondition)condition;
                foreach (ICondition c in rc.Conditions)
                {
                    ImportCondition(c);
                }

                ImportFunction(rc.MetFunction);
            }
            else if (condition is ValueCondition)
            {
                ValueCondition vc = (ValueCondition)condition;
                ImportValue(vc.Value);
                ImportFunction(vc.MetFunction);
            }
        }

        private void ImportTask(ITask task)
        {
            if (task == null) return;
            if (Tasks.ContainsKey(task.GetHashCode().ToString())) return;

            RegistryEntry entry = RegistryEntry.CreateEntry(task);
            Tasks.Add(entry.ID, entry);

            // create condition entries
            ImportCondition(task.StartCondition);
            ImportCondition(task.StopCondition);

            // create value entries
            foreach (IValue v in task.Values)
            {
                ImportValue(v);
            }

            // create run function entry
            ImportFunction(task.RunFunction);

            using (AtomicActivity act = task as AtomicActivity)
            {
                if (act != null)
                {
                    foreach (ITask t in act.Tasks)
                    {
                        ImportTask(t);
                    }
                }

            }
        }

        private void ImportValue(IValue value)
        {
            if (value == null) return;
            if (Values.ContainsKey(value.GetHashCode().ToString())) return;

            RegistryEntry entry = RegistryEntry.CreateEntry(value);

            if (Values.ContainsKey(entry.ID)) return;
            Values.Add(entry.ID, entry);
        }

        private void ImportFunction(Delegate del)
        {
            if (del == null) return;
            if (Functions.ContainsKey(del.GetMethodInfo().GetHashCode().ToString())) return;

            RegistryEntry entry = RegistryEntry.CreateEntry(del);
            Functions.Add(entry.ID, entry);
        }
    }
}
