
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
                    StartCondition = StartEvent.StopCondition,
                    RunFunction = HelloWorldFunctions.DisplayHelloMessage
                }
            };

            StopEvent.StartCondition = new TaskCondition()
            {
                Task = Tasks[0],
                State = TaskState.Done
            };
        }
    }
}
