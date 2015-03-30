using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Atomic.Core;
using System.Reflection;

namespace Atomic.UnitTests.Core
{
    [TestClass]
    public class ProcessTests
    {
        private IProcess _process = null;
        private IContainer _container = null;

        [TestInitialize]
        public void InitializeTest()
        {
            /*
            TaskFunction myFunc = MyFunction;
            MethodInfo meth = myFunc.GetMethodInfo();
            */

            _process = new AtomicProcess()
            {
                Name = "Test Process",
                /*
                Outputs = new IValue[] 
                { 
                    new AtomicValue() { Name = "Count", Value = 0 }
                },
                Tasks = new ITask[] {
                    new GeneralTask() 
                    { 
                        Name = "Starting State Task", 
                        StartCondition = AtomicProcess.TaskStateCondition(_process, RunState.Starting)
                    }, 
                    new GeneralTask() 
                    { 
                        Name = "Stopping State Task", 
                        StartCondition = AtomicProcess.TaskStateCondition(_process, RunState.Stopping)
                    },
                    new GeneralTask() {
                        Name = "Increment Count", 
                        StartCondition = AtomicProcess.TaskStateCondition(_process, RunState.Running), 
                        StopCondition = new ValueCondition() 
                        { 
                            Name = "Equals 3?", 
                            Value = _process.GetOutput("Count"), 
                            ExpectedValue = new AtomicValue() { Value = 3 }
                        }, 
                        RunFunction = new AtomicFunction(meth), 
                        Inputs = new IValueView[] {
                            new IndexView() {
                                Name = "Count", 
                                SourceValue = _process.GetOutput("Count") 
                            }
                        }, 
                        Outputs = new IValue[] { 
                            _process.GetOutput("Count")
                        }
                    }
                }, 
                DoneCondition = AtomicProcess.TaskStateCondition(_process.GetTask("Increment Count"), RunState.Done)
                 * */
            };

            _container = new BasicContainer();
        }

        [TestMethod]
        public void ProcessEmptyProcessRunTest()
        {
            // modify process for test

            // add process to container -- this locks it
            _container.AddProcess(_process);

            // begin testing
            Assert.AreEqual(_process.CurrentState, RunState.Ready);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Starting);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(_process.DoneCondition.Met, true);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.RunComplete);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Stopping);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Done);
        }

        [TestMethod]
        public void ProcessSingleTaskRunTest()
        {
            // modify process for test
            _process.Tasks = new ITask[] 
            {
                new GeneralTask() 
                { 
                    Name = "DefaultTask", 
                    StartCondition = AtomicProcess.TaskStateCondition(_process, RunState.Running) 
                }
            };
            _process.DoneCondition = AtomicProcess.TaskStateCondition(_process.GetTask("DefaultTask"), RunState.Done);

            // add process to container -- this locks it
            _container.AddProcess(_process);

            // begin testing
            ITask defaultTask = _process.GetTask("DefaultTask");

            Assert.AreEqual(_process.CurrentState, RunState.Ready);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Starting);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(defaultTask.CurrentState, RunState.Ready);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(defaultTask.CurrentState, RunState.Running);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(defaultTask.CurrentState, RunState.RunComplete);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(defaultTask.CurrentState, RunState.Done);
            Assert.AreEqual(_process.DoneCondition.Met, true);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.RunComplete);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Stopping);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Done);
        }

        [TestMethod]
        public void ProcessSingleTaskStartTest()
        {
            // modify process for test
            _process.Tasks = new ITask[] 
            {
                new GeneralTask() 
                { 
                    Name = "DefaultTask", 
                    StartCondition = AtomicProcess.TaskStateCondition(_process, RunState.Starting) 
                }
            };
            //_process.DoneCondition = AtomicProcess.TaskStateCondition(_process.GetTask("DefaultTask"), RunState.Done);

            // add process to container -- this locks it
            _container.AddProcess(_process);

            // begin testing
            ITask defaultTask = _process.GetTask("DefaultTask");

            Assert.AreEqual(_process.CurrentState, RunState.Ready);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Starting);
            Assert.AreEqual(defaultTask.CurrentState, RunState.Ready);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Starting);
            Assert.AreEqual(defaultTask.CurrentState, RunState.Running);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Starting);
            Assert.AreEqual(defaultTask.CurrentState, RunState.RunComplete);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Starting);
            Assert.AreEqual(defaultTask.CurrentState, RunState.Done);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.RunComplete);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Stopping);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Done);
        }

        [TestMethod]
        public void ProcessSingleTaskStopTest()
        {
            // modify process for test
            _process.Tasks = new ITask[] 
            {
                new GeneralTask() 
                { 
                    Name = "DefaultTask", 
                    StartCondition = AtomicProcess.TaskStateCondition(_process, RunState.Stopping) 
                }
            };
            //_process.DoneCondition = AtomicProcess.TaskStateCondition(_process.GetTask("DefaultTask"), RunState.Done);

            // add process to container -- this locks it
            _container.AddProcess(_process);

            // begin testing
            ITask defaultTask = _process.GetTask("DefaultTask");

            Assert.AreEqual(_process.CurrentState, RunState.Ready);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Starting);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.RunComplete);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Stopping);
            Assert.AreEqual(defaultTask.CurrentState, RunState.Ready);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Stopping);
            Assert.AreEqual(defaultTask.CurrentState, RunState.Running);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Stopping);
            Assert.AreEqual(defaultTask.CurrentState, RunState.RunComplete);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Stopping);
            Assert.AreEqual(defaultTask.CurrentState, RunState.Done);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Done);
        }

        [TestMethod]
        public void ProcessSingleWithOutputRunTest()
        {
            // modify process for test
            _process.Tasks = new ITask[] 
            {
                new GeneralTask() { 
                    Name = "Counter",                     
                    StartCondition = AtomicProcess.TaskStateCondition(_process, RunState.Running),
                    RunFunction = new AtomicFunction(((TaskFunction)MyFunction).GetMethodInfo())
                }
            };

            ITask counterTask = _process.GetTask("Counter");
            counterTask.Outputs = new IValue[] 
            {
                new AtomicValue() { Name = "Count", Value = 0 }
            };

            counterTask.Inputs = new IValueView[] {
                new IndexView() { Name = "Count", SourceValue = counterTask.GetOutput("Count")}
            };

            counterTask.StopCondition = new ValueCondition()
            {
                Name = "Equals 3?",
                Value = counterTask.GetOutput("Count"),
                ExpectedValue = new AtomicValue() { Value = 3 }
            };

            _process.DoneCondition = AtomicProcess.TaskStateCondition(counterTask, RunState.Done);

            // add process to container -- this locks it
            _container.AddProcess(_process);

            // begin testing
            //ITask defaultTask = _process.GetTask("DefaultTask");

            Assert.AreEqual(_process.CurrentState, RunState.Ready);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Starting);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.CurrentState, RunState.Ready);
            Assert.AreEqual(counterTask.GetInput("Count").Value, 0);
            Assert.AreEqual(counterTask.GetOutput("Count").Value, 0);
            Assert.AreEqual(counterTask.GetOutput("Count").Modified, false);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.GetInput("Count").Value, 0);
            Assert.AreEqual(counterTask.GetOutput("Count").Value, 0);
            Assert.AreEqual(counterTask.GetOutput("Count").Modified, false);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.CurrentState, RunState.RunComplete);
            Assert.AreEqual(counterTask.GetInput("Count").Value, 1);
            Assert.AreEqual(counterTask.GetOutput("Count").Value, 1);
            Assert.AreEqual(counterTask.GetOutput("Count").Modified, true);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.GetInput("Count").Value, 1);
            Assert.AreEqual(counterTask.GetOutput("Count").Value, 1);
            Assert.AreEqual(counterTask.GetOutput("Count").Modified, false);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.CurrentState, RunState.RunComplete);
            Assert.AreEqual(counterTask.GetInput("Count").Value, 2);
            Assert.AreEqual(counterTask.GetOutput("Count").Value, 2);
            Assert.AreEqual(counterTask.GetOutput("Count").Modified, true);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.GetInput("Count").Value, 2);
            Assert.AreEqual(counterTask.GetOutput("Count").Value, 2);
            Assert.AreEqual(counterTask.GetOutput("Count").Modified, false);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.CurrentState, RunState.RunComplete);
            Assert.AreEqual(counterTask.GetInput("Count").Value, 3);
            Assert.AreEqual(counterTask.GetOutput("Count").Value, 3);
            Assert.AreEqual(counterTask.GetOutput("Count").Modified, true);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.CurrentState, RunState.Done);
            Assert.AreEqual(_process.DoneCondition.Met, true);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.RunComplete);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Stopping);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Done);
        }

        [TestMethod]
        public void ProcessSingleWithSharedOutputRunTest()
        {
            // modify process for test
            _process.Tasks = new ITask[] 
            {
                new GeneralTask() { 
                    Name = "Counter",                     
                    StartCondition = AtomicProcess.TaskStateCondition(_process, RunState.Running),
                    RunFunction = new AtomicFunction(((TaskFunction)MyFunction).GetMethodInfo())
                }
            };

            _process.Outputs = new IValue[] 
            {
                new AtomicValue() { Name = "Count", Value = 0 }
            };

            ITask counterTask = _process.GetTask("Counter");
            counterTask.Outputs = new IValue[] 
            {
                _process.GetOutput("Count")
            };

            counterTask.Inputs = new IValueView[] {
                new IndexView() { Name = "Count", SourceValue = counterTask.GetOutput("Count")}
            };

            counterTask.StopCondition = new ValueCondition()
            {
                Name = "Equals 3?",
                Value = counterTask.GetOutput("Count"),
                ExpectedValue = new AtomicValue() { Value = 3 }
            };

            _process.DoneCondition = AtomicProcess.TaskStateCondition(counterTask, RunState.Done);

            // add process to container -- this locks it
            _container.AddProcess(_process);

            // begin testing
            //ITask defaultTask = _process.GetTask("DefaultTask");

            Assert.AreEqual(_process.CurrentState, RunState.Ready);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Starting);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.CurrentState, RunState.Ready);
            Assert.AreEqual(counterTask.GetInput("Count").Value, 0);
            Assert.AreEqual(counterTask.GetOutput("Count").Value, 0);
            Assert.AreEqual(counterTask.GetOutput("Count").Modified, false);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.GetInput("Count").Value, 0);
            Assert.AreEqual(counterTask.GetOutput("Count").Value, 0);
            Assert.AreEqual(counterTask.GetOutput("Count").Modified, false);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.CurrentState, RunState.RunComplete);
            Assert.AreEqual(counterTask.GetInput("Count").Value, 1);
            Assert.AreEqual(counterTask.GetOutput("Count").Value, 1);
            Assert.AreEqual(counterTask.GetOutput("Count").Modified, true);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.GetInput("Count").Value, 1);
            Assert.AreEqual(counterTask.GetOutput("Count").Value, 1);
            Assert.AreEqual(counterTask.GetOutput("Count").Modified, false);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.CurrentState, RunState.RunComplete);
            Assert.AreEqual(counterTask.GetInput("Count").Value, 2);
            Assert.AreEqual(counterTask.GetOutput("Count").Value, 2);
            Assert.AreEqual(counterTask.GetOutput("Count").Modified, true);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.GetInput("Count").Value, 2);
            Assert.AreEqual(counterTask.GetOutput("Count").Value, 2);
            Assert.AreEqual(counterTask.GetOutput("Count").Modified, false);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.CurrentState, RunState.RunComplete);
            Assert.AreEqual(counterTask.GetInput("Count").Value, 3);
            Assert.AreEqual(counterTask.GetOutput("Count").Value, 3);
            Assert.AreEqual(counterTask.GetOutput("Count").Modified, true);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.CurrentState, RunState.Done);
            Assert.AreEqual(_process.DoneCondition.Met, true);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.RunComplete);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Stopping);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Done);
        }

        [TestMethod]
        public void ProcessSingleWithChainedTest()
        {
            // modify process for test
            _process.Outputs = new IValue[] 
            {
                new AtomicValue() { Name = "Count", Value = 0 }
            };

            _process.Tasks = new ITask[] 
            {
                new GeneralTask() { 
                    Name = "Counter",                     
                    StartCondition = AtomicProcess.TaskStateCondition(_process, RunState.Running),
                    RunFunction = new AtomicFunction(((TaskFunction)MyFunction).GetMethodInfo())
                }, 
                new GeneralTask() {
                    Name = "ChainTask", 
                    StartCondition = new ValueCondition() 
                    { 
                        Value = new ValueModifiedView() 
                        { 
                            CompareValue = _process.GetOutput("Count") 
                        }, 
                        ExpectedValue = new AtomicValue() { Value = true }
                    }
                }
            };

            ITask chainTask = _process.GetTask("ChainTask");

            ITask counterTask = _process.GetTask("Counter");
            counterTask.Outputs = new IValue[] 
            {
                _process.GetOutput("Count")
            };

            counterTask.Inputs = new IValueView[] {
                new IndexView() { Name = "Count", SourceValue = counterTask.GetOutput("Count")}
            };

            counterTask.StopCondition = new ValueCondition()
            {
                Name = "Equals 3?",
                Value = counterTask.GetOutput("Count"),
                ExpectedValue = new AtomicValue() { Value = 3 }
            };

            _process.DoneCondition = AtomicProcess.TaskStateCondition(counterTask, RunState.Done);

            // add process to container -- this locks it
            _container.AddProcess(_process);

            // begin testing
            //ITask defaultTask = _process.GetTask("DefaultTask");

            Assert.AreEqual(_process.CurrentState, RunState.Ready);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Starting);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.CurrentState, RunState.Ready);
            Assert.AreEqual(counterTask.GetInput("Count").Value, 0);
            Assert.AreEqual(counterTask.GetOutput("Count").Value, 0);
            Assert.AreEqual(counterTask.GetOutput("Count").Modified, false);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.GetInput("Count").Value, 0);
            Assert.AreEqual(counterTask.GetOutput("Count").Value, 0);
            Assert.AreEqual(counterTask.GetOutput("Count").Modified, false);
            Assert.AreEqual(chainTask.CurrentState, RunState.Ready);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.CurrentState, RunState.RunComplete);
            Assert.AreEqual(counterTask.GetInput("Count").Value, 1);
            Assert.AreEqual(counterTask.GetOutput("Count").Value, 1);
            Assert.AreEqual(counterTask.GetOutput("Count").Modified, true);
            Assert.AreEqual(chainTask.CurrentState, RunState.Running);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.GetInput("Count").Value, 1);
            Assert.AreEqual(counterTask.GetOutput("Count").Value, 1);
            Assert.AreEqual(counterTask.GetOutput("Count").Modified, false);
            Assert.AreEqual(chainTask.CurrentState, RunState.RunComplete);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.CurrentState, RunState.RunComplete);
            Assert.AreEqual(counterTask.GetInput("Count").Value, 2);
            Assert.AreEqual(counterTask.GetOutput("Count").Value, 2);
            Assert.AreEqual(counterTask.GetOutput("Count").Modified, true);
            Assert.AreEqual(chainTask.CurrentState, RunState.Done);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.GetInput("Count").Value, 2);
            Assert.AreEqual(counterTask.GetOutput("Count").Value, 2);
            Assert.AreEqual(counterTask.GetOutput("Count").Modified, false);
            Assert.AreEqual(chainTask.CurrentState, RunState.Ready);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.CurrentState, RunState.RunComplete);
            Assert.AreEqual(counterTask.GetInput("Count").Value, 3);
            Assert.AreEqual(counterTask.GetOutput("Count").Value, 3);
            Assert.AreEqual(counterTask.GetOutput("Count").Modified, true);
            Assert.AreEqual(chainTask.CurrentState, RunState.Running);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.CurrentState, RunState.Done);
            Assert.AreEqual(chainTask.CurrentState, RunState.RunComplete);
            Assert.AreEqual(_process.DoneCondition.Met, true);
            RunCycle();
            Assert.AreEqual(chainTask.CurrentState, RunState.Done);
            Assert.AreEqual(_process.CurrentState, RunState.RunComplete);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Stopping);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Done);
        }

        [TestMethod]
        public void ProcessSingleWithStillRunningRunTest()
        {
            // modify process for test
            _process.Outputs = new IValue[] 
            {
                new AtomicValue() { Name = "Count", Value = 0 }
            };

            _process.Tasks = new ITask[] 
            {
                new GeneralTask() 
                { 
                    Name = "Counter",                     
                    StartCondition = AtomicProcess.TaskStateCondition(_process, RunState.Running),
                    RunFunction = new AtomicFunction(((TaskFunction)MyFunction).GetMethodInfo())
                },
                new GeneralTask() 
                { 
                    Name = "Rogue",                     
                    StartCondition = AtomicProcess.TaskStateCondition(_process, RunState.Running), 
                    StopCondition = new ValueCondition() 
                    { 
                        Value = _process.GetOutput("Count"), 
                        ExpectedValue = new AtomicValue() { Value = 5 } 
                    }
                }
            };

            ITask rogueTask = _process.GetTask("Rogue");
            ITask counterTask = _process.GetTask("Counter");
            counterTask.Outputs = new IValue[] 
            {
                _process.GetOutput("Count")
            };

            counterTask.Inputs = new IValueView[] {
                new IndexView() { Name = "Count", SourceValue = counterTask.GetOutput("Count")}
            };

            counterTask.StopCondition = new ValueCondition()
            {
                Name = "Equals 3?",
                Value = counterTask.GetOutput("Count"),
                ExpectedValue = new AtomicValue() { Value = 3 }
            };

            _process.DoneCondition = AtomicProcess.TaskStateCondition(counterTask, RunState.Done);

            // add process to container -- this locks it
            _container.AddProcess(_process);

            // begin testing
            //ITask defaultTask = _process.GetTask("DefaultTask");

            Assert.AreEqual(_process.CurrentState, RunState.Ready);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Starting);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.CurrentState, RunState.Ready);
            Assert.AreEqual(rogueTask.CurrentState, RunState.Ready);
            Assert.AreEqual(counterTask.GetInput("Count").Value, 0);
            Assert.AreEqual(counterTask.GetOutput("Count").Value, 0);
            Assert.AreEqual(counterTask.GetOutput("Count").Modified, false);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(rogueTask.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.GetInput("Count").Value, 0);
            Assert.AreEqual(counterTask.GetOutput("Count").Value, 0);
            Assert.AreEqual(counterTask.GetOutput("Count").Modified, false);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(rogueTask.CurrentState, RunState.RunComplete);
            Assert.AreEqual(counterTask.CurrentState, RunState.RunComplete);
            Assert.AreEqual(counterTask.GetInput("Count").Value, 1);
            Assert.AreEqual(counterTask.GetOutput("Count").Value, 1);
            Assert.AreEqual(counterTask.GetOutput("Count").Modified, true);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(rogueTask.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.GetInput("Count").Value, 1);
            Assert.AreEqual(counterTask.GetOutput("Count").Value, 1);
            Assert.AreEqual(counterTask.GetOutput("Count").Modified, false);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(rogueTask.CurrentState, RunState.RunComplete);
            Assert.AreEqual(counterTask.CurrentState, RunState.RunComplete);
            Assert.AreEqual(counterTask.GetInput("Count").Value, 2);
            Assert.AreEqual(counterTask.GetOutput("Count").Value, 2);
            Assert.AreEqual(counterTask.GetOutput("Count").Modified, true);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(rogueTask.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.GetInput("Count").Value, 2);
            Assert.AreEqual(counterTask.GetOutput("Count").Value, 2);
            Assert.AreEqual(counterTask.GetOutput("Count").Modified, false);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(rogueTask.CurrentState, RunState.RunComplete);
            Assert.AreEqual(counterTask.CurrentState, RunState.RunComplete);
            Assert.AreEqual(counterTask.GetInput("Count").Value, 3);
            Assert.AreEqual(counterTask.GetOutput("Count").Value, 3);
            Assert.AreEqual(counterTask.GetOutput("Count").Modified, true);
            Assert.AreEqual(_process.DoneCondition.Met, false);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Running);
            Assert.AreEqual(rogueTask.CurrentState, RunState.Running);
            Assert.AreEqual(counterTask.CurrentState, RunState.Done);
            Assert.AreEqual(_process.DoneCondition.Met, true);
            RunCycle();
            Assert.AreEqual(rogueTask.CurrentState, RunState.RunComplete);
            Assert.AreEqual(_process.CurrentState, RunState.RunComplete);
            RunCycle();
            Assert.AreEqual(rogueTask.CurrentState, RunState.Done);
            Assert.AreEqual(_process.CurrentState, RunState.RunComplete);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Stopping);
            RunCycle();
            Assert.AreEqual(_process.CurrentState, RunState.Done);
        }

        private void RunCycle()
        {
            _process.ResetValues();
            _container.Run();
            _process.Update();
        }

        public static void MyFunction(IRunnable task)
        {
            IndexView inputValue = (IndexView)task.GetInput("Count");
            task.GetOutput("Count").Value = inputValue.ViewValue + 1;
        }
    }

}
