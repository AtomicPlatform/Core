using System;
using System.Collections.Generic;
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

        public string Export()
        {
            string EOL = "\r\n";
            StringBuilder buffer = new StringBuilder();
            buffer.Append("define process with name \"{name}\" " + EOL);

            for (int i = 1; i <= Model.Events.Length; i++)
            {
                string evtAttr = "event." + i;
                buffer.Append("add event with name \"{" + evtAttr + "}\" " + EOL);
                buffer.Append("with condition \"{" + evtAttr + ".condition}\" " + EOL);
            }

            for (int i = 1; i <= Model.Conditions.Length; i++)
            {
                string condAttr = "cond." + i;
                buffer.Append("add condition with name \"{" + condAttr + "}\" " + EOL);
                buffer.Append("when task \"{" + condAttr + ".task}\" is \"{" + condAttr + ".state}\" " + EOL);
            }

            buffer.Append("end");

            // do replacement
            buffer.Replace("{name}", Model.ID);

            for (int i = 1; i <= Model.Events.Length; i++)
            {
                string evtAttr = "event." + i;
                EventModel evtModel = Model.Events[i - 1];
                buffer.Replace("{" + evtAttr + "}", evtModel.ID);
                buffer.Replace("{" + evtAttr + ".condition}", evtModel.Condition.ID);
            }

            for (int i = 1; i <= Model.Conditions.Length; i++)
            {
                string condAttr = "cond." + i;
                ConditionModel condModel = Model.Conditions[i - 1];
                buffer.Replace("{" + condAttr + "}", condModel.ID);
                buffer.Replace("{" + condAttr + ".task}", condModel.Task.ID);
                buffer.Replace("{" + condAttr + ".state}", condModel.State.ToString());
            }

            return buffer.ToString();
        }

        public IProcessModel Model
        {
            get { return _model; }
            private set { _model = (ProcessModel)value; }
        }

    }
}
