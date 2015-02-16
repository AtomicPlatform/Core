using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Atomic.Core;
using System.Reflection;

namespace Atomic.Loader
{
    public interface IEntryConverter<T>
    {
        T Export(IDictionary<string, object> attributes);

        IDictionary<string, object> Import(T source);
    }

    public class RegistryEntry
    {
        private IDictionary<string, string> _attributes;
        private object _element = null;

        static public RegistryEntry CreateEntry(IElement element)
        {
            RegistryEntry entry = new RegistryEntry();
            entry.Element = element;
            entry.ExtractElementAttributes();

            if (element is IActivity)
            {
                entry.ExtractActivityAttributes();
            }
            else if (element is ICondition)
            {
                entry.ExtractConditionAttributes();
            }
            else if (element is IProcess)
            {
                entry.ExtractProcessAttributes();
            }
            else if (element is ITask)
            {
                entry.ExtractTaskAttributes();
            }
            else if (element is IValue)
            {
                entry.ExtractValueAttributes();
            }

            return entry;
        }

        static public RegistryEntry CreateEntry(Delegate del)
        {
            RegistryEntry entry = new RegistryEntry();
            entry.Element = del.GetMethodInfo();
            entry._attributes["id"] = entry.Element.GetHashCode().ToString();
            entry.ExtractFunctionAttributes();

            return entry;
        }

        static public RegistryEntry CreateEntry(IDictionary<string, string> attributes)
        {
            RegistryEntry entry = new RegistryEntry();
            entry._attributes = attributes;

            return entry;
        }

        static public MethodInfo ToFunction(RegistryEntry entry)
        {
            entry.CombineFunctionAttributes();

            return (MethodInfo)entry.Element;
        }

        static public IElement ToElement(RegistryEntry entry)
        {
            IElement element = null;

            if (entry.Element == null)
            {
                entry.CombineElementAttributes();
                element = (IElement)entry.Element;

                if (element is IActivity)
                {
                    entry.CombineActivityAttributes();
                }
                else if (element is ICondition)
                {
                    entry.CombineConditionAttributes();
                }
                else if (element is IProcess)
                {
                    entry.CombineProcessAttributes();
                }
                else if (element is ITask)
                {
                    entry.CombineTaskAttributes();
                }
                else if (element is IValue)
                {
                    entry.CombineValueAttributes();
                }
            }
            else
            {
                element = (IElement)entry.Element;
            }

            return element;
        }

        private string GetQualifiedName(object o)
        {
            if (o == null) return "";
            return o.GetType().AssemblyQualifiedName;
        }

        private string GetElementId(object e)
        {
            if (e == null) return "";

            return e.GetHashCode().ToString();
        }

        private RegistryEntry()
        {
            _attributes = new Dictionary<string, string>();
        }

        private void ExtractElementAttributes()
        {
            IElement e = (IElement)_element;

            _attributes["name"] = e.Name;
            _attributes["id"] = GetElementId(e);
            _attributes["elementType"] = GetQualifiedName(e);
        }

        private void CombineElementAttributes()
        {
            // create element instance
            Type elementType = Type.GetType(Attribute("elementType"));

            IElement e = (IElement)Activator.CreateInstance(elementType);
            e.Name = Attribute("name");

            _element = e;
        }

        private void ExtractActivityAttributes()
        {
            IActivity a = (IActivity)_element;

            ExtractTaskAttributes();

            // create task reference list
            List<string> taskList = new List<string>();
            foreach (ITask task in a.Tasks)
            {
                taskList.Add(GetElementId(task));
            }
            _attributes["tasks"] = ArrayToString(taskList);
        }

        private void CombineActivityAttributes()
        {
            IActivity a = (IActivity)_element;
            CombineTaskAttributes();

            string[] tasks = Attribute("tasks").Split(',');
            List<ITask> taskList = new List<ITask>();
            foreach (string taskId in tasks)
            {
                taskList.Add(CreateTaskReference(taskId));
            }

            a.Tasks = taskList.ToArray();
        }

        private void ExtractConditionAttributes()
        {
            _attributes["category"] = "condition";

            ICondition c = (ICondition)_element;

            if (c is TaskCondition)
            {
                TaskCondition tc = (TaskCondition)c;
                _attributes["function"] = GetElementId(tc.MetFunction.GetMethodInfo());
                _attributes["task"] = GetElementId(tc.Task);
            }
            else if (c is ValueCondition)
            {
                ValueCondition vc = (ValueCondition)c;
                _attributes["function"] = GetElementId(vc.MetFunction.GetMethodInfo());
                _attributes["value"] = GetElementId(vc.Value);
            }
            else if (c is RuleCondition)
            {
                RuleCondition rc = (RuleCondition)c;
                List<string> conditionList = new List<string>();
                foreach (ICondition ic in rc.Conditions)
                {
                    conditionList.Add(GetElementId(ic));
                }

                _attributes["function"] = GetElementId(rc.MetFunction.GetMethodInfo());
                _attributes["conditions"] = ArrayToString(conditionList);
            }
        }

        private void CombineConditionAttributes()
        {
            ICondition c = (ICondition)_element;

            if (c is TaskCondition)
            {
                TaskCondition tc = (TaskCondition)c;
                string functionID = Attribute("function");
                tc.MetFunction = (Func<ITask, bool>)CreateFunctionReference(typeof(Func<ITask, bool>), functionID);

                string taskID = Attribute("task");
                tc.Task = CreateTaskReference(taskID);
            }
            else if (c is ValueCondition)
            {
                ValueCondition vc = (ValueCondition)c;
                string functionID = Attribute("function");
                vc.MetFunction = (Func<IValue, bool>)CreateFunctionReference(typeof(Func<IValue, bool>), functionID);

                string valueID = Attribute("value");
                vc.Value = CreateValueReference(valueID);
            }
            else if (c is RuleCondition)
            {
                RuleCondition rc = (RuleCondition)c;
                string functionID = Attribute("function");
                rc.MetFunction = (Func<ICondition[], bool>)CreateFunctionReference(typeof(Func<ICondition[], bool>), functionID);

                string[] conditions = Attribute("conditions").Split(',');
                List<ICondition> conditionList = new List<ICondition>();
                foreach (string conditionID in conditions)
                {
                    conditionList.Add(CreateConditionReference(conditionID));
                }

                rc.Conditions = conditionList.ToArray();
            }
        }

        private ICondition CreateConditionReference(string id)
        {
            if (id == "") return null;
            RegistryEntry entry = Registry.Conditions[id];
            entry.Registry = Registry;
            ICondition condition = (ICondition)RegistryEntry.ToElement(entry);

            return condition;
        }

        private void ExtractProcessAttributes()
        {
            _attributes["category"] = "process";
            _attributes["elementType"] = typeof(AtomicProcess).AssemblyQualifiedName;

            IProcess p = (IProcess)_element;

            // create task reference list
            List<string> taskList = new List<string>();
            _attributes["startEvent"] = GetElementId(p.StartEvent);
            _attributes["stopEvent"] = GetElementId(p.StopEvent);

            foreach (ITask task in p.Events)
            {
                taskList.Add(GetElementId(task));
            }

            _attributes["events"] = ArrayToString(taskList);
        }

        private void CombineProcessAttributes()
        {
            IProcess p = (IProcess)_element;

            string startEventID = Attribute("startEvent");
            ITask startTask = CreateTaskReference(startEventID);
            CloneTask(startTask, p.StartEvent);
            Registry.Tasks.Where(x => x.Key == startEventID).First().Value.Element = p.StartEvent;

            string stopEventID = Attribute("stopEvent");
            ITask stopTask = CreateTaskReference(stopEventID);
            CloneTask(stopTask, p.StopEvent);
            Registry.Tasks.Where(x => x.Key == stopEventID).First().Value.Element = p.StopEvent;

            string[] tasks = Attribute("events").Split(',');
            List<ITask> taskList = new List<ITask>();
            foreach (string taskId in tasks)
            {
                if (taskId == "") continue;
                taskList.Add(CreateTaskReference(taskId));
            }

            p.Events = taskList.ToArray();
        }

        private void CloneTask(ITask source, ITask target)
        {
            target.Name = source.Name;
            target.RunFunction = source.RunFunction;
            target.StartCondition = source.StartCondition;
            target.StopCondition = source.StopCondition;
            target.Values = source.Values;

            using (TaskCondition tc = target.StartCondition as TaskCondition)
            {
                if (tc != null && tc.Task == source) tc.Task = target;
            }

            using (TaskCondition tc = target.StopCondition as TaskCondition)
            {
                if (tc != null && tc.Task == source) tc.Task = target;
            }
        }

        private void ExtractTaskAttributes()
        {
            _attributes["category"] = "task";

            ITask t = (ITask)_element;

            // create value reference list
            List<string> valueList = new List<string>();
            foreach (IValue value in t.Values)
            {
                valueList.Add(GetElementId(value));
            }
            _attributes["values"] = ArrayToString(valueList);

            // create condition reference list
            _attributes["startCondition"] = GetElementId(t.StartCondition);
            _attributes["stopCondition"] = GetElementId(t.StopCondition);

            // add function reference
            _attributes["function"] = GetElementId(t.RunFunction.GetMethodInfo());
        }

        private void CombineTaskAttributes()
        {
            ITask t = (ITask)_element;

            // create value reference list
            string[] values = Attribute("values").Split(',');
            List<IValue> valueList = new List<IValue>();
            foreach (string valueID in values)
            {
                if (valueID == "") continue;
                valueList.Add(CreateValueReference(valueID));
            }
            t.Values = valueList.ToArray();

            // create condition reference list
            string startConditionID = Attribute("startCondition");
            t.StartCondition = CreateConditionReference(startConditionID);

            string stopConditionID = Attribute("stopCondition");
            t.StopCondition = CreateConditionReference(stopConditionID);

            // add function reference
            string functionID = Attribute("function");
            t.RunFunction = (TaskFunction)CreateFunctionReference(typeof(TaskFunction), functionID);
        }

        private ITask CreateTaskReference(string id)
        {
            if (id.Length == 0) return null;

            RegistryEntry taskEntry = Registry.Tasks[id];
            taskEntry.Registry = Registry;

            return (ITask)RegistryEntry.ToElement(taskEntry);
        }

        private void ExtractValueAttributes()
        {
            _attributes["category"] = "value";

            IValue v = (IValue)_element;
            Newtonsoft.Json.JsonSerializerSettings settings = new Newtonsoft.Json.JsonSerializerSettings();
            settings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            _attributes["value"] = Newtonsoft.Json.JsonConvert.SerializeObject(v.Value, settings);
            _attributes["valueType"] = GetQualifiedName(v.Value);
        }

        private void CombineValueAttributes()
        {
            IValue v = (IValue)_element;

            Type t = Type.GetType(Attribute("valueType"));
            //object o = Activator.CreateInstance(t);
            string valueString = Attribute("value");

            v.Value = Newtonsoft.Json.JsonConvert.DeserializeObject(valueString);
        }

        private IValue CreateValueReference(string id)
        {
            if (id.Length == 0) return null;

            RegistryEntry entry = Registry.Values[id];
            entry.Registry = Registry;

            return (IValue)RegistryEntry.ToElement(entry);
        }

        private void ExtractFunctionAttributes()
        {
            MethodInfo meth = (MethodInfo)_element;
            _attributes["category"] = "function";

            _attributes["methodName"] = meth.Name;
            _attributes["className"] = meth.DeclaringType.AssemblyQualifiedName;

            string assemblyName = meth.Module.Name;
            assemblyName = assemblyName.Substring(0, assemblyName.LastIndexOf("."));
            _attributes["assemblyName"] = assemblyName;
        }

        private void CombineFunctionAttributes()
        {
            string qualifiedName = Attribute("assemblyName");
            AssemblyName asmName = new AssemblyName(qualifiedName);
            Assembly asm = Assembly.Load(asmName);

            string className = Attribute("className");
            string methodName = Attribute("methodName");

            Type methodType = (Type)asm.ExportedTypes.Where(x => x.AssemblyQualifiedName == className).FirstOrDefault();
            MethodInfo meth = methodType.GetRuntimeMethods().Where(x => x.Name.Equals(methodName)).FirstOrDefault();

            _element = meth;
        }

        private Delegate CreateFunctionReference(Type functionType, string id)
        {
            RegistryEntry functionEntry = Registry.Functions[id];
            MethodInfo meth = RegistryEntry.ToFunction(functionEntry);

            return meth.CreateDelegate(functionType);
        }

        private string ArrayToString(IEnumerable<string> array)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in array)
            {
                if (array.First() != s) sb.Append(",");
                sb.Append(s);
            }

            return sb.ToString();
        }

        public string ID
        {
            get { return Attribute("id"); }
        }

        public object Element
        {
            get { return _element; }
            set { _element = value; }
        }

        public ElementRegistry Registry { get; set; }

        public IDictionary<string, string> Attributes
        {
            get
            {
                return _attributes;
            }
        }

        public string Attribute(string name)
        {
            if (_attributes.ContainsKey(name))
            {
                return _attributes[name];
            }
            else
            {
                return "";
            }
        }
    }
}
