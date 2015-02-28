using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;
using System.IO;
using System.Collections.Generic;

using Atomic.Loader;

namespace Atomic.UnitTests.Loader
{
    [TestClass]
    public class XmlConverterTests
    {
        private IDataConverter dc = null;

        [TestInitialize]
        public void InitializeTest()
        {
            dc = new XmlConverter();
        }

        [TestCleanup]
        public void CleanupTest()
        {

        }

        private string InitializeXmlText()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("HelloWorld.xml");

            string xmlText = doc.InnerXml;
            doc = null;

            return xmlText;
        }

        private XmlProcessModel InitializeModel()
        {
            XmlProcessModel model = new XmlProcessModel();
            model.ID = "hello_world";
            model.Name = "Hello World";

            /*
            model.Events = new EventModel[] {
                new EventModel() { ID = "_stop", StartConditionID = "taskDone" }
            };
            model.Tasks = new TaskModel[] {
                new TaskModel() { ID = "display_greeting", Name = "Display Greeting", RunScript="print \"Hello World!\"", StartConditionID = "startDone" };
            };
            model.Conditions = new ConditionModel[] {
                new ConditionModel() { ID = "startDone", TaskID="_start", State = ConditionModel.TaskState.Done }, 
                new ConditionModel() { ID = "taskDone", TaskID="display_message", State = ConditionModel.TaskState.Done }
            };
    */

            return model;
        }

        [TestMethod]
        public void ExportNativeXmlInitialStateTest()
        {
            XmlConverter convert = new XmlConverter();
            XmlProcessModel model = InitializeModel();

            
            model.Events = new EventListModel()
            {
                Event = new EventModel[] {
                    new EventModel() { ID = "_stop", Condition = new RefIdModel() { ID = "taskDone" } }
                }
            };
            
            model.Tasks = new TaskListModel()
            {
                Task = new TaskModel[] 
                {
                    new TaskModel() 
                    { 
                        ID = "display_greeting", 
                        Name = "Display Greeting", 
                        RunScript="print \"Hello World!\"", 
                        //StartCondition = new RefIdModel() { ID = "startDone" },
                        //StopCondition = new RefIdModel()
                    }
                }
            };
            /*
            model.Conditions = new ConditionListModel() 
            { 
                Condition = new ConditionModel[] {
                    new ConditionModel() { 
                        ID = "startDone", 
                        Task = new RefIdModel() { ID = "_start" }, 
                        State = ConditionModel.TaskState.Done 
                    }, 
                    new ConditionModel() { 
                        ID = "taskDone", 
                        Task = new RefIdModel() { ID = "display_message"}, 
                        State = ConditionModel.TaskState.Done 
                    }
                }
            };
//            */

            string s = convert.Export();
        }
        /*
        [TestMethod]
        public void ExportXmlInitialStateTest()
        {
            ProcessModel model = InitializeModel();
            string resultText = dc.Export(model);
            Atomic.Loader.XmlElement root = ((XmlConverter)dc).Element;

            Assert.AreEqual(root.Namespaces.Count, 1);
            Assert.AreEqual(root.Namespaces["atomic"], "http://www.atomicplatform.com/Process");
            Assert.AreEqual(root.Attributes["id"], "hello_world");
            Assert.AreEqual(root.Attributes["name"], "Hello World");
            Assert.AreEqual(root.Children.Count, 3);
            Assert.AreEqual(root.Children[0].Children.Count, 0);
            Assert.AreEqual(root.Children[1].Children.Count, 0);
            Assert.AreEqual(root.Children[2].Children.Count, 0);

            Assert.IsTrue(resultText.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>"));
            Assert.IsTrue(resultText.Contains("xmlns:atomic=\"http://www.atomicplatform.com/Process\""));
            Assert.IsTrue(resultText.Contains("id=\"hello_world\""));
            Assert.IsTrue(resultText.Contains("name=\"Hello World\""));
            Assert.IsTrue(resultText.Contains("<atomic:process"));
            Assert.IsTrue(resultText.Contains("<atomic:events />"));
            Assert.IsTrue(resultText.Contains("<atomic:tasks />"));
            Assert.IsTrue(resultText.Contains("<atomic:conditions />"));
            Assert.IsTrue(resultText.EndsWith("</atomic:process>"));
        }

        [TestMethod]
        public void ExportXmlEventTest()
        {
            ProcessModel model = InitializeModel();
            model.Events = new EventModel[] {
                new EventModel() { ID = "_stop", StartConditionID = "taskDone" }
            };
            string resultText = dc.Export(model);
            Atomic.Loader.XmlElement root = ((XmlConverter)dc).Element;

            Assert.AreEqual(root.Children.Count, 3);
            Assert.AreEqual(root.Children[0].TagName, "atomic:events");
            Assert.AreEqual(root.Children[0].Children.Count, 1);

            Atomic.Loader.XmlElement child = root.Children[0].Children[0];
            Assert.AreEqual(child.TagName, "atomic:event");
            Assert.AreEqual(child.Attributes["id"], "_stop");
            Assert.AreEqual(child.Attributes["name"], "");
            Assert.AreEqual(child.Children.Count, 1);

            child = child.Children[0];
            Assert.AreEqual(child.TagName, "startOnCondition");
            Assert.AreEqual(child.Attributes["id"], "taskDone");
 
            Assert.IsTrue(resultText.Contains("<atomic:events>"));
            Assert.IsTrue(resultText.Contains("<atomic:event"));
            Assert.IsTrue(resultText.Contains("</atomic:event>"));
            Assert.IsTrue(resultText.Contains("</atomic:events>"));

        }
        */

        [TestMethod]
        public void ImportNativeXmlInitialStateTest()
        {
            XmlConverter convert = new XmlConverter();
            StreamReader reader = new StreamReader(new FileStream("HelloWorld.xml", FileMode.Open));
            string xmlText = reader.ReadToEnd();
            reader.Close();
        }

        /*
         * <?xml version="1.0" encoding="utf-8"?>
<atomic:process xmlns:atomic="http://www.atomicplatform.com/Process"
            id="hello_world" name="Hello World">
<atomic:events>
<atomic:event id="_stop">
  <startOnCondition id="taskDone" />
</atomic:event>
</atomic:events>
<atomic:tasks>
<atomic:task id="display_greeting" name="Display Greeting">
  <startOnCondition id="startDone" />
  <runScript>print "Hello world!"</runScript>
</atomic:task>
</atomic:tasks>
<atomic:conditions>
<atomic:taskCondition id="startDone" name="Start Done">
  <task id="_start" />
  <state>Done</state>
</atomic:taskCondition>
<atomic:taskCondition id="taskDone" name="Task Done">
  <task id="display_greeting" />
  <state>Done</state>
</atomic:taskCondition>
</atomic:conditions>
</atomic:process>
         */
    }
}
