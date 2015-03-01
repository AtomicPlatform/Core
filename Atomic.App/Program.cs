using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Atomic.Core;
using Atomic.Loader;
using Atomic.Loader.Xml;

namespace Atomic.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // define model
            //IProcess p = new Atomic.Samples.HelloWorld();
            IProcess p = new Atomic.Core.AtomicProcess();

            // do a test run
            RunCoreProgram(p);

            Console.WriteLine("\nPress any key to continue.");
            Console.ReadKey();

            // export JSON model
            JsonConverter jsonConvert = new JsonConverter();
            ExportToFile(jsonConvert, p, ".json");

            // export XML model
            XmlConverter xmlConvert = new XmlConverter();
            ExportToFile(xmlConvert, p, ".xml");

            // export BPMN xml
            XslTransformConverter bpmnConvert = new XslTransformConverter("../../XSL/AppXML-To-BPMN.xslt");
            ExportToFile(bpmnConvert, p, ".bpmn20.xml");

            // export plain text 
            PlainTextConverter textConvert = new PlainTextConverter();
            ExportToFile(textConvert, p, ".txt");

            /*
            IProcess p = new Atomic.Samples.Countdown() { Name = "Countdown" };
            Atomic.Loader.ElementRegistry reg = new Atomic.Loader.ElementRegistry();

            reg.Import(p);

            XmlEntryConverter xmlConverter = new XmlEntryConverter();
            JsonEntryConverter jsonConverter = new JsonEntryConverter();

            ExportRegistry(reg);

            Atomic.Loader.ElementRegistry newReg = ImportRegistry("Countdown");
            IProcess serializedProcess = (IProcess)RegistryEntry.ToElement(newReg.Process);

            serializedProcess.StartEvent.Values[0].Value = Console.Out;

            RunCoreProgram(serializedProcess);
            */

            Console.WriteLine("\nPress any key to continue.");
            Console.ReadKey();
        }

        /*
        private static void ExportRegistry(Atomic.Loader.ElementRegistry reg)
        {
            IDictionary<string, object> exportData = reg.Export();

            // Export as JSON text
            JsonEntryConverter jsonConverter = new JsonEntryConverter();
            string appName = reg.Process.Attribute("name");
            string samplesDir = "../../../Atomic.Samples/";
            DirectoryInfo dir = new DirectoryInfo(samplesDir);

            string filename = dir.FullName + appName + "/" + appName + ".txt";

            StreamWriter writer = new StreamWriter(filename, false, Encoding.UTF8)
            {
                AutoFlush = true
            };

            writer.Write(jsonConverter.Export(exportData));
            writer.Close();

            // Export as XML text
            XmlEntryConverter xmlConverter = new XmlEntryConverter();
            filename = dir.FullName + appName + "/" + appName + ".xml";

            writer = new StreamWriter(filename, false, Encoding.UTF8)
            {
                AutoFlush = true
            };

            writer.Write(xmlConverter.Export(exportData));
            writer.Close();
        }

        static Atomic.Loader.ElementRegistry ImportRegistry(string appName)
        {
            Atomic.Loader.ElementRegistry reg = new Atomic.Loader.ElementRegistry();
            string samplesDir = "../../../Atomic.Samples/";
            DirectoryInfo dir = new DirectoryInfo(samplesDir);

            string filename = dir.FullName + appName + "/" + appName + ".txt";
            StreamReader reader = new StreamReader(filename);

            // read content from file
            string content = reader.ReadToEnd();
            reader.Close();

            // get import structure
            JsonEntryConverter jsonConverter = new JsonEntryConverter();
            IDictionary<string, object> importData = jsonConverter.Import(content);

            reg.Import(importData);
            reg.Process.Registry = reg;

            return reg;
        }
        */

        static void RunCoreProgram(Atomic.Core.IProcess p)
        {
            bool done = false;
            int step = 0;
            p.Locked = true;

            while (!done)
            {
                Console.WriteLine("Current step: {0}...", (step++));
                p.Run();
                p.Update();

                done = (p.CurrentState == Atomic.Core.TaskState.Done);
            }
        }
        // * */

        static private void ExportToFile(IDataConverter convert, IProcess p, string fileExt)
        {
            // create JSON model
            IProcessModel model = convert.Model;
            model.Import(p);

            //  create JSON text
            string exportText = convert.Export();

            // write JSON text to file
            FileStream fs = null;
            StreamWriter writer = null;

            fs = new FileStream("../../Samples/" + p.Name + fileExt, FileMode.Create);
            writer = new StreamWriter(fs);
            writer.Write(exportText);
            writer.Flush();
            writer.Close();
        }
    }


}
