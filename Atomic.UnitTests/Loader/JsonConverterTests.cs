using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Atomic.Loader;
using System.Collections.Generic;

namespace Atomic.UnitTests.Loader
{
    [TestClass]
    public class JsonConverterTests
    {
        private JsonConverter dc = null;

        private ProcessModel InitializeModel(ProcessModel model)
        {
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
            ProcessModel model = (ProcessModel)dc.Model;
            InitializeModel(model);
            string resultText = dc.Export();

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
            ProcessModel model = (ProcessModel)dc.Model;
            InitializeModel(model);
            model.Events = new EventModel[] {
                    new EventModel() { ID = "_stop", StartCondition = new RefIdModel() { ID = "taskDone" } }
            };

            string resultText = dc.Export();
            Assert.IsTrue(resultText.Contains("\"Events\":[{"));
            Assert.IsTrue(resultText.Contains("\"ID\":\"_stop\""));
            Assert.IsTrue(resultText.Contains("\"Name\":\"\""));
            Assert.IsTrue(resultText.Contains("\"StartCondition\":{\"ID\":\"taskDone\"}"));
            Assert.IsTrue(resultText.Contains("\"StopCondition\":null}"));
        }

        [TestMethod]
        public void ExportJsonTaskTest()
        {
            ProcessModel model = (ProcessModel)dc.Model;
            InitializeModel(model);
            model.Tasks = new TaskModel[] {
                    new TaskModel() { 
                        ID = "display_greeting", 
                        Name = "Display Greeting", 
                        RunScript="print \"Hello World!\"", 
                        StartCondition = new RefIdModel() { ID = "startDone" }
                    }
            };

            string resultText = dc.Export();
            Assert.IsTrue(resultText.Contains("\"Tasks\":[{"));
            Assert.IsTrue(resultText.Contains("\"ID\":\"display_greeting\""));
            Assert.IsTrue(resultText.Contains("\"Name\":\"Display Greeting\""));
            Assert.IsTrue(resultText.Contains("\"StartCondition\":{\"ID\":\"startDone\"}"));
            Assert.IsTrue(resultText.Contains("\"StopCondition\":null"));
            Assert.IsTrue(resultText.Contains("\"RunScript\":\"print \\\"Hello World!\\\"\""));
        }

        [TestMethod]
        public void ExportJsonConditionTest()
        {
            ProcessModel model = (ProcessModel)dc.Model;
            InitializeModel(model);
            model.Conditions = new ConditionModel[] {
                    new ConditionModel() { 
                        ID = "startDone", 
                        Task = new RefIdModel() { ID = "_start" }, 
                        State = "Done" 
                    }, 
                    new ConditionModel() { 
                        ID = "taskDone", 
                        Task = new RefIdModel() { ID = "display_message" }, 
                        State = "Done" 
                    }
            };

            string resultText = dc.Export();
            Assert.IsTrue(resultText.Contains("\"Conditions\":[{"));
            Assert.IsTrue(resultText.Contains("\"ID\":\"startDone\""));
            Assert.IsTrue(resultText.Contains("\"Task\":{\"ID\":\"_start\"}"));
            Assert.IsTrue(resultText.Contains("\"State\":0"));
            Assert.IsTrue(resultText.Contains("\"ID\":\"taskDone\""));
            Assert.IsTrue(resultText.Contains("\"Task\":{\"ID\":\"display_message\"}"));
            Assert.IsTrue(resultText.Contains("\"State\":0"));
        }

        [TestMethod]
        public void ImportJsonInitialStateTest()
        {
            string jsonText = "{\"Name\":\"Hello World\",\"ID\":\"hello_world\",\"Events\":null,\"Tasks\":null,\"Conditions\":null}";
            dc.Import(jsonText);
            ProcessModel model = (ProcessModel)dc.Model;

            Assert.AreEqual(model.ID, "hello_world");
            Assert.AreEqual(model.Name, "Hello World");
            Assert.AreEqual(model.Events.Length, 0);
            Assert.AreEqual(model.Tasks.Length, 0);
            Assert.AreEqual(model.Conditions.Length, 0);
        }

        [TestMethod]
        public void ImportJsonEventTest()
        {
            string jsonText = "{\"Name\":null,\"ID\":null,\"Events\":[{\"ID\":\"_stop\",\"Name\":null,\"StartCondition\":{\"ID\":\"taskDone\"},\"StopCondition\":{\"ID\":\"\"}}],\"Tasks\":null,\"Conditions\":null}";
            dc.Import(jsonText);
            ProcessModel model = (ProcessModel)dc.Model;

            Assert.AreEqual(model.ID, "");
            Assert.AreEqual(model.Name, "");
            Assert.AreEqual(model.Events.Length, 1);
            Assert.AreEqual(model.Events[0].ID, "_stop");
            Assert.AreEqual(model.Events[0].Name, "");
            Assert.AreEqual(model.Events[0].StartCondition.ID, "taskDone");
            Assert.AreEqual(model.Tasks.Length, 0);
            Assert.AreEqual(model.Conditions.Length, 0);
        }

        [TestMethod]
        public void ImportJsonTaskTest()
        {
            string jsonText = "{\"Name\":null,\"ID\":null,\"Events\":null,\"Tasks\":[{\"ID\":\"display_greeting\",\"Name\":\"Display Greeting\",\"StartCondition\":{\"ID\":\"startDone\"},\"StopCondition\":{\"ID\":\"\"},\"RunScript\":\"print \\\"Hello World!\\\"\"}],\"Conditions\":null}";
            dc.Import(jsonText);
            ProcessModel model = (ProcessModel)dc.Model;

            Assert.AreEqual(model.ID, "");
            Assert.AreEqual(model.Name, "");
            Assert.AreEqual(model.Events.Length, 0);
            Assert.AreEqual(model.Tasks.Length, 1);
            Assert.AreEqual(model.Tasks[0].ID, "display_greeting");
            Assert.AreEqual(model.Tasks[0].Name, "Display Greeting");
            Assert.AreEqual(model.Tasks[0].StartCondition.ID, "startDone");
            Assert.AreEqual(model.Tasks[0].StopCondition.ID, "");
            Assert.AreEqual(model.Tasks[0].RunScript, "print \"Hello World!\"");
            Assert.AreEqual(model.Conditions.Length, 0);
        }

        [TestMethod]
        public void ImportJsonConditionTest()
        {
            ProcessModel model = (ProcessModel)dc.Model;
            InitializeModel(model);
            model.Conditions = new ConditionModel[] {
                    new ConditionModel() { 
                        ID = "startDone", 
                        Task = new RefIdModel() { ID = "_start" }, 
                        State = "Done" 
                    }, 
                    new ConditionModel() { 
                        ID = "taskDone", 
                        Task = new RefIdModel() { ID = "display_message" }, 
                        State = "Done" 
                    }
            };

            string jsonText = dc.Export();
            dc.Import(jsonText);

            Assert.AreEqual(model.ID, "hello_world");
            Assert.AreEqual(model.Name, "Hello World");
            Assert.AreEqual(model.Events.Length, 0);
            Assert.AreEqual(model.Tasks.Length, 0);
            Assert.AreEqual(model.Conditions.Length, 2);

            Assert.AreEqual(model.Conditions[0].ID, "startDone");
            Assert.AreEqual(model.Conditions[0].Name, "");
            Assert.AreEqual(model.Conditions[0].Task.ID, "_start");
            Assert.AreEqual(model.Conditions[0].State, "Done");

            Assert.AreEqual(model.Conditions[1].ID, "taskDone");
            Assert.AreEqual(model.Conditions[1].Name, "");
            Assert.AreEqual(model.Conditions[1].Task.ID, "display_message");
            Assert.AreEqual(model.Conditions[1].State, "Done");
        }
    }
}
