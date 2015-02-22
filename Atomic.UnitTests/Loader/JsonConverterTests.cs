using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Atomic.Loader;
using System.Collections.Generic;

namespace Atomic.UnitTests.Loader
{
    [TestClass]
    public class JsonConverterTests
    {
        private IDataConverter dc = null;

        private ProcessModel InitializeModel()
        {
            ProcessModel model = new ProcessModel();
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

        private string InitializeJsonText()
        {
            return "";
        }

        [TestInitialize]
        public void InitializeTest()
        {
            dc = new JsonConverter();
        }

        [TestCleanup]
        public void CleanupTest()
        {

        }

        [TestMethod]
        public void ExportJsonInitialStateTest()
        {
            ProcessModel model = InitializeModel();
            string resultText = dc.Export(model);

            Assert.IsTrue(resultText.StartsWith("{"));
            Assert.IsTrue(resultText.Contains("\"Name\":\"Hello World\""));
            Assert.IsTrue(resultText.Contains("\"ID\":\"hello_world\""));
            Assert.IsTrue(resultText.Contains("\"Events\":[]"));
            Assert.IsTrue(resultText.Contains("\"Tasks\":[]"));
            Assert.IsTrue(resultText.Contains("\"Conditions\":[]"));
            Assert.IsTrue(resultText.EndsWith("}"));
        }

        [TestMethod]
        public void ExportJsonEventTest()
        {
            ProcessModel model = InitializeModel();
            model.Events = new EventModel[] {
                new EventModel() { ID = "_stop", StartConditionID = "taskDone" }
            };

            string resultText = dc.Export(model);
            Assert.IsTrue(resultText.Contains("\"Events\":[{"));
            Assert.IsTrue(resultText.Contains("\"ID\":\"_stop\""));
            Assert.IsTrue(resultText.Contains("\"Name\":\"\""));
            Assert.IsTrue(resultText.Contains("\"StartConditionID\":\"taskDone\""));
            Assert.IsTrue(resultText.Contains("\"StopConditionID\":\"\""));
        }

        [TestMethod]
        public void ExportJsonTaskTest()
        {
            ProcessModel model = InitializeModel();
            model.Tasks = new TaskModel[] {
                new TaskModel() { ID = "display_greeting", Name = "Display Greeting", RunScript="print \"Hello World!\"", StartConditionID = "startDone" }
            };

            string resultText = dc.Export(model);
            Assert.IsTrue(resultText.Contains("\"Tasks\":[{"));
            Assert.IsTrue(resultText.Contains("\"ID\":\"display_greeting\""));
            Assert.IsTrue(resultText.Contains("\"Name\":\"Display Greeting\""));
            Assert.IsTrue(resultText.Contains("\"StartConditionID\":\"startDone\""));
            Assert.IsTrue(resultText.Contains("\"StopConditionID\":\"\""));
            Assert.IsTrue(resultText.Contains("\"RunScript\":\"print \\\"Hello World!\\\"\""));
        }

        [TestMethod]
        public void ExportJsonConditionTest()
        {
            ProcessModel model = InitializeModel();
            model.Conditions = new ConditionModel[] {
                new ConditionModel() { ID = "startDone", TaskID="_start", State = ConditionModel.TaskState.Done }, 
                new ConditionModel() { ID = "taskDone", TaskID="display_message", State = ConditionModel.TaskState.Done }
            };

            string resultText = dc.Export(model);
            Assert.IsTrue(resultText.Contains("\"Conditions\":[{"));
            Assert.IsTrue(resultText.Contains("\"ID\":\"startDone\""));
            Assert.IsTrue(resultText.Contains("\"TaskID\":\"_start\""));
            Assert.IsTrue(resultText.Contains("\"State\":0"));
            Assert.IsTrue(resultText.Contains("\"ID\":\"taskDone\""));
            Assert.IsTrue(resultText.Contains("\"TaskID\":\"display_message\""));
            Assert.IsTrue(resultText.Contains("\"State\":0"));
        }

        [TestMethod]
        public void ImportJsonInitialStateTest()
        {
            string jsonText = "{\"Name\":\"Hello World\",\"ID\":\"hello_world\",\"Events\":null,\"Tasks\":null,\"Conditions\":null}";
            ProcessModel model = dc.Import(jsonText);

            Assert.AreEqual(model.ID, "hello_world");
            Assert.AreEqual(model.Name, "Hello World");
            Assert.AreEqual(model.Events.Length, 0);
            Assert.AreEqual(model.Tasks.Length, 0);
            Assert.AreEqual(model.Conditions.Length, 0);
        }

        [TestMethod]
        public void ImportJsonEventTest()
        {
            string jsonText = "{\"Name\":null,\"ID\":null,\"Events\":[{\"ID\":\"_stop\",\"Name\":null,\"StartConditionID\":\"taskDone\",\"StopConditionID\":null}],\"Tasks\":null,\"Conditions\":null}";
            ProcessModel model = dc.Import(jsonText);

            Assert.AreEqual(model.ID, "");
            Assert.AreEqual(model.Name, "");
            Assert.AreEqual(model.Events.Length, 1);
            Assert.AreEqual(model.Events[0].ID, "_stop");
            Assert.AreEqual(model.Events[0].Name, "");
            Assert.AreEqual(model.Events[0].StartConditionID, "taskDone");
            Assert.AreEqual(model.Events[0].StopConditionID, "");
            Assert.AreEqual(model.Tasks.Length, 0);
            Assert.AreEqual(model.Conditions.Length, 0);
        }

        [TestMethod]
        public void ImportJsonTaskTest()
        {
            string jsonText = "{\"Name\":null,\"ID\":null,\"Events\":null,\"Tasks\":[{\"ID\":\"display_greeting\",\"Name\":\"Display Greeting\",\"StartConditionID\":\"startDone\",\"StopConditionID\":null,\"RunScript\":\"print \\\"Hello World!\\\"\"}],\"Conditions\":null}";
            ProcessModel model = dc.Import(jsonText);

            Assert.AreEqual(model.ID, "");
            Assert.AreEqual(model.Name, "");
            Assert.AreEqual(model.Events.Length, 0);
            Assert.AreEqual(model.Tasks.Length, 1);
            Assert.AreEqual(model.Tasks[0].ID, "display_greeting");
            Assert.AreEqual(model.Tasks[0].Name, "Display Greeting");
            Assert.AreEqual(model.Tasks[0].StartConditionID, "startDone");
            Assert.AreEqual(model.Tasks[0].StopConditionID, "");
            Assert.AreEqual(model.Tasks[0].RunScript, "print \"Hello World!\"");
            Assert.AreEqual(model.Conditions.Length, 0);
        }

        [TestMethod]
        public void ImportJsonConditionTest()
        {
            string jsonText = "{\"Name\":null,\"ID\":null,\"Events\":null,\"Tasks\":null,\"Conditions\":[{\"ID\":\"startDone\",\"Name\":null,\"TaskID\":\"_start\",\"State\":0},{\"ID\":\"taskDone\",\"Name\":null,\"TaskID\":\"display_greeting\",\"State\":0}]}";
            ProcessModel model = dc.Import(jsonText);

            Assert.AreEqual(model.ID, "");
            Assert.AreEqual(model.Name, "");
            Assert.AreEqual(model.Events.Length, 0);
            Assert.AreEqual(model.Tasks.Length, 0);
            Assert.AreEqual(model.Conditions.Length, 2);

            Assert.AreEqual(model.Conditions[0].ID, "startDone");
            Assert.AreEqual(model.Conditions[0].Name, "");
            Assert.AreEqual(model.Conditions[0].TaskID, "_start");
            Assert.AreEqual(model.Conditions[0].State, ConditionModel.TaskState.Done);

            Assert.AreEqual(model.Conditions[1].ID, "taskDone");
            Assert.AreEqual(model.Conditions[1].Name, "");
            Assert.AreEqual(model.Conditions[1].TaskID, "display_greeting");
            Assert.AreEqual(model.Conditions[1].State, ConditionModel.TaskState.Done);
        }
    }
}
