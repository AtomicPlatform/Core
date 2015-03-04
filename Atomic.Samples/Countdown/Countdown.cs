
using Atomic.Core;

namespace Atomic.Samples
{
    public class Countdown : AtomicProcess
    {
        public Countdown()
        {
            /*
            StartEvent.Values = new IValue[] 
            {
                new AtomicValue() { Name = "outputStream" }
            };
            */
            IActivity activity = new AtomicActivity() { Name = "Start" };
            /*
            activity.Values = new IValue[] 
            {
                StartEvent.Values[0]
            };
            */
            AtomicTask countdownTask = new AtomicTask();
            countdownTask.Values = new IValue[] 
            {
                new AtomicValue() { Name = "index", Value = 10 }
            };
            countdownTask.RunFunction = CountdownFunctions.DecrementCount;
            countdownTask.StartCondition = new TaskCondition()
            {
                Task = activity,
                State = TaskState.Running
            };
            countdownTask.StopCondition = new ValueCondition()
            {
                Value = countdownTask.Values[0],
                ExpectedValue = new AtomicValue() {  Value = 0 }
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
                Value = new ValueModifiedView() { CompareValue = countdownTask.Values[0] },
                ExpectedValue = new AtomicValue { Value = true }
            };

            displayCount.StopCondition = new RuleCondition()
            {
                MetFunction = CoreFunctions.OneConditionMet,
                Conditions = new ICondition[] 
                {
                    new TaskCondition()
                    {
                        Task = countdownTask,
                        State = TaskState.Done
                    },
                    new ValueCondition()
                    {
                        Value = countdownTask.Values[0],
                        ExpectedValue = new AtomicValue() { Value = 0 }
                    }
                }
            };

            /*
            activity.StartCondition = new RuleCondition()
            {
                Conditions = new ICondition[] {
                    StartEvent.Condition
                }, 
                MetFunction = CoreFunctions.AllConditionsMet
            };
             */ 
            activity.Tasks = new ITask[] { displayCount, countdownTask };
            activity.StopCondition = new TaskCondition()
            {
                Task = countdownTask,
                State = TaskState.Done
            };

            /*
            StopEvent.Condition = new TaskCondition()
            {
                Task = activity,
                State = TaskState.Done
            };
            */
            Tasks = new ITask[] { activity };
        }
    }

}
