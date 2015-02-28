using System.Linq;

using Atomic.Core;
using System.IO;

namespace Atomic.Samples
{
    static public class HelloWorldFunctions
    {
        static public void DisplayHelloMessage(IRunnable task)
        {
            // get output stream
            IValue streamValue = task.Values.Where(x => x.Name == "outputStream").First();
            string message = "Hello World!";

            if (streamValue.Value is TextWriter)
            {
                TextWriter writer = (TextWriter)streamValue.Value;
                writer.WriteLine(message);
            }
            else if (streamValue.Value is Stream)
            {
                StreamWriter writer = new StreamWriter((Stream)streamValue.Value);
                writer.WriteLine(message);
            }
        }
    }
}
