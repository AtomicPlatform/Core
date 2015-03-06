using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Atomic.Loader
{
    public class PlainTextConverter : IDataConverter
    {
        private static string ProcessTemplateText = "define process with name \"$ID\" ";
        private static string AddTemplateText = "add $elementType with name \"$ID\" ";
        private static string TaskStateMetText = "when task \"$Task.ID\" state is \"$State\" ";
        private static string ValueMetText = "when value \"$Value.ID\" is \"$ExpectedValue\" ";

        private IProcessModel _model = new ProcessModel();

        public string FileExtension { get { return "txt"; } }

        public void Import(string sourceText)
        {
            Regex elementRegEx = new Regex("/(define|add)\\s\\w+/g");
            Match elementMatch = elementRegEx.Match(sourceText);
            while (elementMatch.Success) 
            {
                Regex nameRegEx = new Regex("/(with\\s+name)\\s+\"\\s*\\w+\\s*\"/g");
                Match nameMatch = nameRegEx.Match(sourceText, elementMatch.Index);
                elementMatch = elementMatch.NextMatch();
            }
            /*
             * 
/(define|add)\s\w+/g => gets "define process", "add condition", "add event"
/(with\s+name)\s+"\s*\w+\s*"/g => gets name portion
/(start|stop)\s+on\s+condition\s+"\s*\w+\s*"/g => start|stop on condition name
/when\s+task\s+"\s*\w+\"\s+is\s+"\s*\w+\s*\"/g => task condition clause
             */ 

            /*
            bool isDone = false;
            string[] lines = sourceText.Split('\n');
            foreach (string line in lines)
            {
                string[] tokens = line.Trim().Split(' ');
                switch (tokens[0].ToLower())
                {
                    case "define":
                        if (tokens[1] == "process") InitializeModel(tokens);
                        break;
                    case "add":
                        if (tokens[1] == "event") CreateEvent(tokens);
                        if (tokens[1] == "condition") CreateCondition(tokens);
                        break;
                    case "end":
                        isDone = true;
                        break;
                    default:
                        break;
                }

                if (isDone)
            }*/

        }

        public string Export()
        {
            StringBuilder buffer = new StringBuilder();
            WriteText(buffer, ProcessTemplateText.Replace("$ID", Model.Name), 0, true);

            /*
            foreach (EventModel evtModel in Model.Events)
            {
                buffer.AppendLine(AddTemplateText
                    .Replace("$ID", evtModel.Name)
                    .Replace("$elementType", "event")
                );

                switch (evtModel.EventType)
                {
                    case 
                }
                buffer.AppendLine(StartConditionTemplateText
                    .Replace("$ID", evtModel.Name)
                    .Replace("$StartCondition.ID", evtModel.StartCondition.ID)
                );
            }
            */

            foreach (TaskModel taskModel in Model.Tasks)
            {
                string line = AddTemplateText
                    .Replace("$elementType", "task")
                    .Replace("$ID", taskModel.Name);

                WriteText(buffer, line, 1, true);

                if (taskModel.StartCondition != null)
                {
                    WriteConditionText(buffer, "start", taskModel.StartCondition, 2);
                }

                if (taskModel.StopCondition != null)
                {
                    WriteConditionText(buffer, "end", taskModel.StopCondition, 2);
                }

                WriteText(buffer, taskModel.RunScript, 2, true);
            }

            ConditionModel doneModel = Model.Conditions.Where(x => x.ID == "_done").First();
            RefIdModel doneRefModel = new RefIdModel() { ID = doneModel.ID };
            WriteConditionText(buffer, "end", doneRefModel, 0);

            return buffer.ToString();
        }

        private void WriteConditionText(StringBuilder buffer, string mode, RefIdModel idModel, int indent)
        {
            string line = mode + " ";
            ConditionModel condModel = Model.Conditions.Where(x => x.ID == idModel.ID).FirstOrDefault();
            if (condModel != null)
            {
                switch (condModel.ConditionType)
                {
                    case "TaskCondition":
                        line += TaskStateMetText
                            .Replace("$Task.ID", condModel.Task.ID)
                            .Replace("$State", condModel.State);
                        break;
                    case "ValueCondition":
                        line += GetValueText(condModel.Value, condModel.ExpectedValue);

                        break;
                    default:
                        break;
                }

                WriteText(buffer, line, indent, true);
            }
        }

        private string GetValueText(RefIdModel valueRefID, string expectedValue)
        {
            ValueModel valModel = Model.Values.Where(x => x.ID == valueRefID.ID).FirstOrDefault();
            string line = "";
            if (valModel != null)
            {
                switch (valModel.ValueType)
                {
                    case "TaskStateView":
                        line = TaskStateMetText
                            .Replace("$Task.ID", valModel.Task.ID)
                            .Replace("$State", expectedValue);
                        break;
                    case "ConditionMetView":
                        break;
                    case "ValueModifiedView":
                        break;
                    default:
                         line += ValueMetText
                            .Replace("$Value.ID", valModel.ID)
                            .Replace("$ExpectedValue", expectedValue);
                       break;
                }
            }

            return line;
        }

        private void WriteText(StringBuilder buffer, string line, int indent, bool eol)
        {
            for (int i = 0; i < indent; i++)
            {
                buffer.Append("    ");
            }

            buffer.Append(line);

            if (eol) buffer.AppendLine();
        }

        public IProcessModel Model
        {
            get { return _model; }
        }
    }
}
