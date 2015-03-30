using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atomic.Core
{

    /// <summary>
    /// A task element that contains a group of related tasks as an activity.
    /// </summary>
    /// <remarks>An activity allows for encapsulating tasks, preventing other tasks from
    /// accessing internal task states and values.  To make such information known, these
    /// values can be linked to the activity value collection.</remarks>
    public interface IActivity : ITask
    {
        /// <summary>
        /// A collection of tasks in the activity.
        /// </summary>
        ITask[] Tasks { get; set; }
    }

    public class AtomicActivity : AtomicTask, IActivity
    {
        private List<ITask> _tasks = new List<ITask>();

        public AtomicActivity()
        {
        }

        public override void Run()
        {
            base.Run();

            foreach (ITask t in Tasks)
            {
                t.Run();
            }
        }

        public override void Update()
        {
            base.Update();

            foreach (ITask t in Tasks)
            {
                t.Update();
            }
        }

        public ITask[] Tasks
        {
            get { return _tasks.ToArray(); }
            set { _tasks.Clear(); _tasks.AddRange(value); }
        }

        public override bool Locked
        {
            get { return base.Locked; }
            set
            {
                base.Locked = value;

                foreach (ITask t in Tasks)
                {
                    t.Locked = value;
                }
            }
        }
    }

    public interface IFileTask : ITask { }

    public interface IGeneralTask : ITask { }

    public interface IPresentationTask : ITask { }

    public class FileTask : AtomicTask { }

    public class GeneralTask : AtomicTask { }

    public class PresentationTask : AtomicTask { }

    public class SendMessageTask : AtomicTask
    {
        public IProcess TargetProcess { get; set; }

        public IMessage Message { get; set; }

        public SendMessageTask()
        {
            //RunFunction = SendMessageTask.SendMessage;
        }

        static public void SendMessage(IRunnable task)
        {
            SendMessageTask sendTask = (SendMessageTask)task;
            string eventName = (string)sendTask.Message.GetParameter("Name").Value;
            MessageEvent evt = (MessageEvent)sendTask.TargetProcess.GetEvent(eventName);
            evt.Message = sendTask.Message;
        }
    }
}
