using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atomic.Loader
{
    public class XmlConverter : IDataConverter
    {
        private XmlElement _root = new XmlElement();

        public ProcessModel Import(string sourceText)
        {
            throw new NotImplementedException();
        }

        public XmlElement Element
        {
            get { return _root; }
        }

        public string Export(ProcessModel sourceModel)
        {
            string xmlHeading = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";

            _root = new XmlElement();
            _root.TagName = "atomic:process";
            _root.Namespaces["atomic"] = "http://www.atomicplatform.com/Process";
            _root.Attributes["id"] = sourceModel.ID;
            _root.Attributes["name"] = sourceModel.Name;
            _root.Children.Add(CreateEventElement(sourceModel.Events));
            _root.Children.Add(CreateTaskElement(sourceModel.Tasks));
            _root.Children.Add(CreateConditionElement(sourceModel.Conditions));

            return xmlHeading + _root.ToString();
        }

        private XmlElement CreateEventElement(EventModel[] events)
        {
            XmlElement element = new XmlElement() { TagName = "atomic:events" };

            foreach (EventModel evt in events)
            {
                XmlElement evtElement = new XmlElement() { TagName = "atomic:event" };
                evtElement.Attributes["id"] = evt.ID;
                evtElement.Attributes["name"] = evt.Name;

                XmlElement condElement = new XmlElement();
                if (evt.StartConditionID.Length > 0)
                {
                    condElement = new XmlElement() { TagName = "startOnCondition" };
                    condElement.Attributes["id"] = evt.StartConditionID;
                    evtElement.Children.Add(condElement);
                }

                if (evt.StopConditionID.Length > 0)
                {
                    condElement = new XmlElement() { TagName = "stopOnCondition" };
                    condElement.Attributes["id"] = evt.StopConditionID;
                    evtElement.Children.Add(condElement);
                }

                element.Children.Add(evtElement);
            }

            return element;
        }

        private XmlElement CreateTaskElement(TaskModel[] tasks)
        {
            XmlElement element = new XmlElement() { TagName = "atomic:tasks" };

            foreach (TaskModel task in tasks)
            {
                XmlElement taskElement = new XmlElement() { TagName = "atomic:task" };
                taskElement.Attributes["id"] = task.ID;
                taskElement.Attributes["name"] = task.Name;

                XmlElement condElement = new XmlElement();
                if (task.StartConditionID.Length > 0)
                {
                    condElement = new XmlElement() { TagName = "startOnCondition" };
                    condElement.Attributes["id"] = task.StartConditionID;
                    taskElement.Children.Add(condElement);
                }

                if (task.StopConditionID.Length > 0)
                {
                    condElement = new XmlElement() { TagName = "stopOnCondition" };
                    condElement.Attributes["id"] = task.StopConditionID;
                    taskElement.Children.Add(condElement);
                }

                taskElement.Children.Add(new XmlElement() { TagName = "runScript", Text = task.RunScript });
                element.Children.Add(taskElement);
            }

            return element;
        }

        private XmlElement CreateConditionElement(ConditionModel[] conditions)
        {
            XmlElement element = new XmlElement() { TagName = "atomic:conditions" };

            foreach (ConditionModel cond in conditions)
            {
                XmlElement condElement = new XmlElement() { TagName = "atomic:condition" };
                condElement.Attributes["id"] = cond.ID;
                condElement.Attributes["name"] = cond.Name;

                XmlElement taskElement = new XmlElement();
                if (cond.TaskID.Length > 0)
                {
                    taskElement = new XmlElement() { TagName = "taskID" };
                    taskElement.Attributes["id"] = cond.TaskID;
                    condElement.Children.Add(taskElement);

                    taskElement = new XmlElement() { TagName = "state", Text = Enum.GetName(typeof(ConditionModel.TaskState), cond.State) };
                    condElement.Children.Add(taskElement);
                }

                element.Children.Add(condElement);
            }

            return element;
        }
    }

    public struct XmlElement
    {
        private string _tagName;
        private IDictionary<string, string> _namespaces;
        private IDictionary<string, string> _attributes;
        private List<XmlElement> _children;
        private string _text;

        public XmlElement(string xmlText)
        {
            _tagName = "";
            _namespaces = new Dictionary<string, string>();
            _attributes = new Dictionary<string, string>();
            _children = new List<XmlElement>();
            _text = "";

            List<string> tagList = new List<string>();
            StringBuilder buffer = new StringBuilder(xmlText.Trim());

            while (buffer.Length > 0)
            {
                int startPos = buffer.ToString().IndexOf('<');
                int endPos = buffer.ToString().IndexOf('>');

                if (startPos > 0)
                {
                    string tagContent = buffer.ToString(0, startPos).Trim();
                    if (tagContent.Length > 0) tagList.Add(tagContent);
                }

                tagList.Add(buffer.ToString().Substring(startPos, endPos - startPos));
                buffer.Remove(0, endPos);
            }

            PopulateXmlElement(tagList);
        }

        public string TagName
        {
            get { if (_tagName == null) _tagName = ""; return _tagName; }
            set { if (value == null) value = ""; _tagName = value.Trim(); }
        }

        public string Text
        {
            get { if (_text == null) _text = ""; return _text; }
            set { if (value == null) value = ""; _text = value.Trim(); }
        }

        public List<XmlElement> Children
        {
            get { if (_children == null) _children = new List<XmlElement>(); return _children; }
            set { if (value == null) value = new List<XmlElement>(); _children = value; }
        }

        public IDictionary<string, string> Attributes
        {
            get { if (_attributes == null) _attributes = new Dictionary<string, string>(); return _attributes; }
            set { if (value == null) value = new Dictionary<string, string>(); _attributes = value; }
        }

        public IDictionary<string, string> Namespaces
        {
            get { if (_namespaces == null) _namespaces = new Dictionary<string, string>(); return _namespaces; }
            set { if (value == null) value = new Dictionary<string, string>(); _namespaces = value; }
        }

        override public string ToString()
        {
            // make sure everything is initialized
            if (TagName == null) TagName = "";
            if (Namespaces == null) Namespaces = new Dictionary<string, string>();
            if (Attributes == null) Attributes = new Dictionary<string, string>();
            if (Children == null) Children = new List<XmlElement>();
            if (Text == null) Text = "";

            Text = Text.Trim();

            StringBuilder builder = new StringBuilder();
            string namespaceText = GetNamespaceText();
            string attributeText = GetAttributeText();

            builder.Append("<" + TagName);
            if (namespaceText.Length > 0) builder.Append(namespaceText);
            if (attributeText.Length > 0) builder.Append(attributeText);

            if (Children.Count == 0 && Text.Length == 0)
            {
                builder.Append(" />");
            }
            else
            {
                builder.Append(">");

                foreach (XmlElement child in Children)
                {
                    builder.Append(child);
                }

                builder.Append(Text);
                builder.Append("</" + TagName + ">");
            }

            return builder.ToString();
        }

        private string GetNamespaceText()
        {
            if (Namespaces == null || Namespaces.Count == 0) return "";

            StringBuilder buffer = new StringBuilder();
            foreach (KeyValuePair<string, string> keyPair in Namespaces)
            {
                buffer.Append(" xmlns:" + keyPair.Key + "=\"" + keyPair.Value + "\"");
            }

            return buffer.ToString();
        }

        private string GetAttributeText()
        {
            if (Attributes == null || Attributes.Count == 0) return "";

            StringBuilder buffer = new StringBuilder();
            foreach (KeyValuePair<string, string> keyPair in Attributes)
            {
                buffer.Append(" " + keyPair.Key + "=\"" + keyPair.Value + "\"");
            }

            return buffer.ToString();
        }

        private void PopulateXmlElement(List<string> tagList)
        {

        }
    }
}
