﻿
using Atomic.Core;

namespace Atomic.Samples
{
    public class Countdown : AtomicProcess
    {
        public Countdown()
        {
            StartEvent.Values = new IValue[] 
            {
                new AtomicValue() { Name = "outputStream" }
            };

            IActivity activity = new AtomicActivity() { Name = "Start" };
            activity.Values = new IValue[] 
            {
                StartEvent.Values[0]
            };

            AtomicTask countdownTask = new AtomicTask();
            countdownTask.Values = new IValue[] 
            {
                new AtomicValue() { Name = "index", Value = 10 }
            };
            countdownTask.RunFunction = CountdownFunctions.DecrementCount;
            countdownTask.StartCondition = new TaskCondition()
            {
                Task = activity,
                MetFunction = CountdownFunctions.TaskStateRunning
            };
            countdownTask.StopCondition = new ValueCondition()
            {
                Value = countdownTask.Values[0],
                MetFunction = CountdownFunctions.AtZero
            };

            AtomicTask displayCount = new AtomicTask();
            displayCount.Name = "DisplayNameTask";
            displayCount.Values = new IValue[] 
            {
                countdownTask.Values[0],
                activity.Values[0]
            };
            displayCount.RunFunction = CountdownFunctions.DisplayCurrentCount;
            displayCount.StartCondition = new ValueCondition()
            {
                Value = countdownTask.Values[0],
                MetFunction = CountdownFunctions.CountChanged
            };

            displayCount.StopCondition = new RuleCondition()
            {
                MetFunction = CoreFunctions.OneConditionMet,
                Conditions = new ICondition[] 
                {
                    new TaskCondition()
                    {
                        Task = countdownTask,
                        MetFunction = CountdownFunctions.TaskStateDone
                    },
                    new ValueCondition()
                    {
                        Value = countdownTask.Values[0],
                        MetFunction = CountdownFunctions.AtZero
                    }
                }
            };

            activity.StartCondition = new TaskCondition()
            {
                Task = StartEvent,
                MetFunction = CoreFunctions.TaskDone
            };
            activity.Tasks = new ITask[] { displayCount, countdownTask };
            activity.StopCondition = new TaskCondition()
            {
                Task = countdownTask,
                MetFunction = CountdownFunctions.TaskStateDone
            };

            StopEvent.StartCondition = new TaskCondition()
            {
                Task = activity,
                MetFunction = CoreFunctions.TaskDone
            };

            Events = new ITask[] { activity };
        }
    }

}