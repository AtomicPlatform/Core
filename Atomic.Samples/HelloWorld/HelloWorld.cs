
using Atomic.Core;

namespace Atomic.Samples
{
    public class HelloWorld : AtomicProcess
    {
        public HelloWorld()
        {
            Tasks = new ITask[] {
                new AtomicTask() 
                { 
                    Name = "Display Greeting", 
                    StartCondition = StartEvent.Condition,
                    RunFunction = HelloWorldFunctions.DisplayHelloMessage
                }
            };

            StopEvent.Condition = new TaskCondition()
            {
                Task = Tasks[0],
                State = TaskState.Done
            };
        }
    }
}
