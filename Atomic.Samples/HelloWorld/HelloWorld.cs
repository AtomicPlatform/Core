
using Atomic.Core;

namespace Atomic.Samples
{
    public class HelloWorld : AtomicProcess
    {
        public HelloWorld()
        {
            StartEvent.Values = new IValue[] 
            {
                new AtomicValue() { Name = "outputStream" }
            };
            StartEvent.RunFunction = HelloWorldFunctions.DisplayHelloMessage;

            StopEvent.StartCondition = new TaskCondition()
            {
                Task = StartEvent,
                MetFunction = CoreFunctions.TaskDone
            };
        }
    }
}
