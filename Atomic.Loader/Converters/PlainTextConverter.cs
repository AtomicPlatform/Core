using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atomic.Loader
{
    public class PlainTextConverter : IDataConverter
    {
        private IProcessModel _model = new ProcessModel();

        public void Import(string sourceText)
        {
            throw new NotImplementedException();
        }

        private static string ProcessTemplateText = "define process with name \"{ID}\"";
        private static string EventTemplateText = "add event with name \"{ID}\" start on condition \"{StartCondition.ID}\" stop on condition \"{StopCondition.ID}\" ";
        private static string ConditionTemplateText = "add condition with name \"{ID}\" when ";
        private static string TaskConditionTemplateText = "task \"{Task.ID}\" is \"{State}\"";
        private static string TaskTemplateText = "";
        private static string ValueTemplateText = "";

        public string Export()
        {
            StringBuilder buffer = new StringBuilder();
            buffer.AppendLine(ProcessTemplateText.Replace("{ID}", Model.ID));

            buffer.AppendLine(EventTemplateText
                .Replace("{ID}", Model.StartEvent.ID)
                .Replace("{StartCondition.ID}", Model.StartEvent.StartCondition.ID)
                .Replace("{StopCondition.ID}", Model.StartEvent.StopCondition.ID)
            );

            buffer.AppendLine(EventTemplateText
                .Replace("{ID}", Model.StopEvent.ID)
                .Replace("{StartCondition.ID}", Model.StopEvent.StartCondition.ID)
                .Replace("{StopCondition.ID}", Model.StopEvent.StopCondition.ID)
            );

            foreach (EventModel evtModel in Model.Events)
            {
                buffer.AppendLine(EventTemplateText
                    .Replace("{ID}", evtModel.ID)
                    .Replace("{StartCondition.ID}", evtModel.StartCondition.ID)
                    .Replace("{StopCondition.ID}", evtModel.StopCondition.ID)
                );
            }

            foreach (TaskModel taskModel in Model.Tasks)
            {
                buffer.AppendLine(TaskTemplateText
                );
            }

            foreach (ConditionModel condModel in Model.Conditions)
            {
                buffer.Append(ConditionTemplateText
                    .Replace("{ID}", condModel.ID)
                );

                if (condModel.Task != null) {
                    buffer.AppendLine(TaskConditionTemplateText
                        .Replace("{Task.ID}", condModel.Task.ID)
                        .Replace("{State}", condModel.State.ToString())
                    );
                }
            }

            foreach (ValueModel valueModel in Model.Values)
            {
                buffer.AppendLine(ValueTemplateText
                );
            }

            buffer.AppendLine("end");

            return buffer.ToString();
        }

        public IProcessModel Model
        {
            get { return _model; }
        }
    }
}
