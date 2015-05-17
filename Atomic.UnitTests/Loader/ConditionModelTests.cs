using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Atomic.Core;
using Atomic.Loader;

namespace Atomic.UnitTests.Loader
{
    [TestClass]
    public class ConditionModelTests
    {
        private ConditionModel _model = null;
        private IProcessModel _process = null;

        [TestInitialize]
        public void InitializeTest()
        {
            _model = new ConditionModel();
            _process = new ProcessModel();
        }

        [TestMethod]
        public void ConditionModelInitialTest()
        {
            Assert.AreEqual(_model.ConditionType, null);
            Assert.AreEqual(_model.ExpectedValue, null);
            Assert.AreEqual(_model.Function, null);
            Assert.AreEqual(_model.ID, null);
            Assert.AreEqual(_model.Name, null);
            Assert.AreEqual(_model.State, null);
            Assert.AreEqual(_model.Task, null);
            Assert.AreEqual(_model.Value, null);
        }

        [TestMethod]
        public void ConditionModelDefaultValueTest()
        {
            ICondition cond = new ValueCondition() { Name = "TestCondition" };
            _model.Element = cond;

            Assert.AreEqual(_model.ConditionType, "ValueCondition");
            Assert.AreEqual(_model.ExpectedValue, Undefined.Value.ToString());
            Assert.AreEqual(_model.Function.ID, "testcondition_func");
            Assert.AreEqual(_model.ID, "testcondition");
            Assert.AreEqual(_model.Name, "TestCondition");
            Assert.AreEqual(_model.State, null);
            Assert.AreEqual(_model.Task, null);
            Assert.AreEqual(_model.Value.ID, Undefined.Value.ID);

            Assert.AreEqual(_process.Functions.Length, 1);
            Assert.AreEqual(_process.Functions[0].ID, _model.Function.ID);
            Assert.AreEqual(_process.Functions[0].FunctionText, null);
            Assert.AreEqual(_process.Functions[0].AssemblyName, cond.GetType().Assembly.FullName);
            Assert.AreEqual(_process.Functions[0].MethodName, "EqualsFunction");
            Assert.AreEqual(_process.Functions[0].ModuleName, "ValueCondition");
        }


        [TestMethod]
        public void ConditionModelCompleteValueTest()
        {
            ICondition cond = new ValueCondition()
            {
                Name = "TestCondition",
                Value = new AtomicValue() { Name = "Source", Value = 5 },
                ExpectedValue = new AtomicValue { Name = "Expected", Value = "hello" }
            };
            _model.Element = cond;
            
            Assert.AreEqual(_model.ConditionType, "ValueCondition");
            Assert.AreEqual(_model.ExpectedValue, "hello");
            Assert.AreEqual(_model.Function.ID, "testcondition_func");
            Assert.AreEqual(_model.ID, "testcondition");
            Assert.AreEqual(_model.Name, "TestCondition");
            Assert.AreEqual(_model.State, null);
            Assert.AreEqual(_model.Task, null);
            Assert.AreEqual(_model.Value.ID, "source");

            Assert.AreEqual(_process.Functions.Length, 1);
            Assert.AreEqual(_process.Functions[0].ID, _model.Function.ID);
            Assert.AreEqual(_process.Functions[0].FunctionText, null);
            Assert.AreEqual(_process.Functions[0].AssemblyName, cond.GetType().Assembly.FullName);
            Assert.AreEqual(_process.Functions[0].MethodName, "EqualsFunction");
            Assert.AreEqual(_process.Functions[0].ModuleName, "ValueCondition");

            Assert.AreEqual(_process.Values.Length, 1);
            Assert.AreEqual(_process.Values[0].Name, "Source");
            Assert.AreEqual(_process.Values[0].CompareValue, null);
            Assert.AreEqual(_process.Values[0].Condition, null);
            Assert.AreEqual(_process.Values[0].Task, null);
            Assert.AreEqual(_process.Values[0].Value, "5");
            Assert.AreEqual(_process.Values[0].ValueType, "AtomicValue");
            Assert.AreEqual(_process.Values[0].ParameterType, null);
            Assert.AreEqual(_process.Values[0].Required, false);
        }

        [TestMethod]
        public void ConditionModelDefaultTaskTest()
        {
            ICondition cond = new TaskCondition() { Name = "TestCondition" };
            _model.Element = cond;

            Assert.AreEqual(_model.ConditionType, "TaskCondition");
            Assert.AreEqual(_model.ExpectedValue, null);
            Assert.AreEqual(_model.Function.ID, "testcondition_func");
            Assert.AreEqual(_model.ID, "testcondition");
            Assert.AreEqual(_model.Name, "TestCondition");
            Assert.AreEqual(_model.State, "Done");
            Assert.AreEqual(_model.Task.ID, Undefined.Task.ID);
            Assert.AreEqual(_model.Value, null);

            Assert.AreEqual(_process.Functions.Length, 1);
            Assert.AreEqual(_process.Functions[0].ID, _model.Function.ID);
            Assert.AreEqual(_process.Functions[0].FunctionText, null);
            Assert.AreEqual(_process.Functions[0].AssemblyName, cond.GetType().Assembly.FullName);
            Assert.AreEqual(_process.Functions[0].MethodName, "EqualsFunction");
            Assert.AreEqual(_process.Functions[0].ModuleName, "TaskCondition");
        }

        [TestMethod]
        public void ConditionModelCompleteTaskTest()
        {
            ITask task = new GeneralTask() { Name = "Task" };
            ICondition cond = new TaskCondition() 
            { 
                Name = "TestCondition", 
                Task = task, 
                State = RunState.Running  
            };
            _model.Element = cond;

            Assert.AreEqual(_model.ConditionType, "TaskCondition");
            Assert.AreEqual(_model.ExpectedValue, null);
           // Assert.AreEqual(_model.Function.ID, "testcondition_func");
            Assert.AreEqual(_model.ID, "testcondition");
            Assert.AreEqual(_model.Name, "TestCondition");
            Assert.AreEqual(_model.State, "Running");
            Assert.AreEqual(_model.Task.ID, "task");
            Assert.AreEqual(_model.Value, null);

            Assert.AreEqual(_process.Functions.Length, 1);
            Assert.AreEqual(_process.Functions[0].ID, _model.Function.ID);
            Assert.AreEqual(_process.Functions[0].FunctionText, null);
            Assert.AreEqual(_process.Functions[0].AssemblyName, cond.GetType().Assembly.FullName);
            Assert.AreEqual(_process.Functions[0].MethodName, "EqualsFunction");
            Assert.AreEqual(_process.Functions[0].ModuleName, "TaskCondition");

            Assert.AreEqual(_process.Tasks.Length, 1);
            Assert.AreEqual(_process.Tasks[0].Name, "Task");
            Assert.AreEqual(_process.Tasks[0].TaskType, "GeneralTask");
        }

        [TestMethod]
        public void ConditionModelToValueObject()
        {
            Func<ValueCondition, bool> methodFunc = ValueCondition.EqualsFunction;
            IFunction func = new AtomicFunction() 
            { 
                Name = "func", 
                Method = methodFunc.GetMethodInfo() 
            };

            IValue val = new AtomicValue() { Name = "counter", Value = 0 };
            ValueCondition cond = new ValueCondition()
            {
                FunctionElement = func,
                ExpectedValue = new AtomicValue() { Value = 4 },
                Value = val,
                Name = "condition"
            };

            FunctionModel funcModel = new FunctionModel() { Element = func };
            ValueModel valModel = new ValueModel() { Element = val };
            ConditionModel condModel = new ConditionModel() { Element = cond };

            ValueCondition modelCond = (ValueCondition)condModel.Element;
            Assert.AreEqual(modelCond.ExpectedValue, cond.ExpectedValue);
            Assert.AreEqual(modelCond.FunctionElement, cond.FunctionElement);
            Assert.AreEqual(modelCond.ID, cond.ID);
            Assert.AreEqual(modelCond.Value, cond.Value);
        }
    }
}
