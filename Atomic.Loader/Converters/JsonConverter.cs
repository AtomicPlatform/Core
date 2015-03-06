using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//using Newtonsoft.Json;

namespace Atomic.Loader
{
    public class JsonConverter : IDataConverter
    {
        private ProcessModel _model = new ProcessModel();

        public string FileExtension { get { return "json"; } }

        public void Import(string sourceText)
        {
            /*
            Model = (ProcessModel)JsonConvert.DeserializeObject(
                sourceText, 
                typeof(ProcessModel)
            );
             * */
        }

        public string Export()
        {
            IDictionary<string, string> content = new Dictionary<string, string>();
            content["id"] = AsJsonString(Model.ID);
            content["name"] = AsJsonString(Model.Name);
            content["events"] = AsJsonCollection(Model.Events);
            content["tasks"] = AsJsonCollection(Model.Tasks);
            content["conditions"] = AsJsonCollection(Model.Conditions);
            content["values"] = AsJsonCollection(Model.Values);
            content["functions"] = AsJsonCollection(Model.Functions);

            return AsJsonString(content);
        }

        public IProcessModel Model
        {
            get { return _model; }
            private set { _model = (ProcessModel)value; }
        }

        private string AsJsonString(string s)
        {
            return "\"" + s + "\"";
        }

        private string AsJsonString(IDictionary<string, string> content)
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("{ ");

            foreach (KeyValuePair<string, string> keyPair in content)
            {
                buf.Append("\"" + keyPair.Key + "\": " + keyPair.Value);
                if (keyPair.Key != content.Keys.Last()) buf.Append(", ");
            }

            buf.Append(" }");

            return buf.ToString();
        }

        private string AsJsonCollection(Array ary)
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("[ ");

            for (int i = 0; i < ary.Length; i++)
            {
                object o = ary.GetValue(i);
                buf.Append(AsJsonObject(o));

                if (i < ary.Length - 1) buf.Append(", ");
            }

            buf.Append(" ]");

            return buf.ToString();
        }

        private string AsJsonObject(object o)
        {
            IDictionary<string, string> content = new Dictionary<string, string>();

            if (o is EventModel) content = AsJsonObject((EventModel)o);
            if (o is TaskModel) content = AsJsonObject((TaskModel)o);
            if (o is ConditionModel) content = AsJsonObject((ConditionModel)o);
            if (o is ValueModel) content = AsJsonObject((ValueModel)o);
            if (o is FunctionModel) content = AsJsonObject((FunctionModel)o);

            return AsJsonString(content);
        }

        private IDictionary<string, string> AsJsonObject(EventModel evtModel)
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();
            dict["id"] = AsJsonString(evtModel.ID);
            dict["name"] = AsJsonString(evtModel.Name);
            dict["eventType"] = AsJsonString(evtModel.EventType);
            dict["startCondition"] = AsJsonString(evtModel.StartCondition.ID);

            return dict;
        }

        private IDictionary<string, string> AsJsonObject(TaskModel taskModel)
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();
            dict["id"] = AsJsonString(taskModel.ID);
            dict["name"] = AsJsonString(taskModel.Name);
            dict["runScript"] = AsJsonString(taskModel.RunScript);
            dict["startCondition"] = AsJsonString(taskModel.StartCondition.ID);
            dict["stopCondition"] = AsJsonString(taskModel.StopCondition.ID);

            return dict;
        }

        private IDictionary<string, string> AsJsonObject(ConditionModel condModel)
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();
            dict["id"] = AsJsonString(condModel.ID);
            dict["name"] = AsJsonString(condModel.Name);
            dict["conditionType"] = AsJsonString(condModel.ConditionType);

            // switch on condition type
            switch (dict["conditionType"])
            {
                case "ValueCondition":
                    dict["expectedValue"] = AsJsonString(condModel.ExpectedValue);
                    dict["value"] = AsJsonString(condModel.Value.ID);
                    dict["function"] = AsJsonString(condModel.Function.ID);

                    break;
                case "TaskCondition":
                    dict["state"] = AsJsonString(condModel.State);
                    dict["task"] = AsJsonString(condModel.Task.ID);

                    break;
                default:
                    break;
            }

            return dict;
        }

        private IDictionary<string, string> AsJsonObject(ValueModel valModel)
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();
            dict["id"] = AsJsonString(valModel.ID);
            dict["name"] = AsJsonString(valModel.Name);
            dict["value"] = AsJsonString(valModel.Value);
            dict["valueType"] = AsJsonString(valModel.ValueType);

            switch (dict["valueType"])
            {
                case "ConditionMetView":
                    dict["condition"] = AsJsonString(valModel.Condition.ID);
                    break;
                case "TaskStateView":
                    dict["task"] = AsJsonString(valModel.Task.ID);
                    break;
                case "ValueModifiedView":
                    dict["compareValue"] = AsJsonString(valModel.CompareValue.ID);
                    break;
                default:
                    break;
            }

            return dict;
        }

        private IDictionary<string, string> AsJsonObject(FunctionModel funcModel)
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();
            dict["id"] = AsJsonString(funcModel.ID);
            dict["name"] = AsJsonString(funcModel.Name);

            dict["assemblyName"] = AsJsonString(funcModel.AssemblyName);
            dict["moduleName"] = AsJsonString(funcModel.ModuleName);
            dict["methodName"] = AsJsonString(funcModel.MethodName);

            return dict;
        }
    }
}
