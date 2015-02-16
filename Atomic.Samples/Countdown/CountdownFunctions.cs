﻿using Atomic.Core;
using System.IO;
using System.Linq;

namespace Atomic.Samples
{
    static public class CountdownFunctions
    {
        static public void DisplayCurrentCount(ITask task)
        {
            IValue countValue = task.Values.Where(x => x.Name == "index").FirstOrDefault();
            long currentCount = (long)countValue.Value;
            string message = currentCount.ToString() + "...";

            IValue streamValue = task.Values.Where(x => x.Name == "outputStream").FirstOrDefault();
            if (streamValue.Value is TextWriter)
            {
                ((TextWriter)streamValue.Value).Write(message);
            }
            else if (streamValue.Value is Stream)
            {
                StreamWriter writer = new StreamWriter((Stream)streamValue.Value);
                writer.Write(message);
            }
        }

        static public void DecrementCount(ITask task)
        {
            IValue countValue = task.Values.Where(x => x.Name == "index").FirstOrDefault();
            countValue.Value = (long)countValue.Value - 1;
        }

        static public bool AtZero(IValue countValue)
        {
            return (long)countValue.Value == 0;
        }

        static public bool TaskStateRunning(ITask task)
        {
            return task.CurrentState == TaskState.Running;
        }

        static public bool TaskStateDone(ITask task)
        {
            return task.CurrentState == TaskState.Done;
        }

        static public bool CountChanged(IValue countValue)
        {
            return countValue.Modified;
        }

        static public bool NotLessThanZero(IValue countValue)
        {
            return (long)countValue.Value >= 0;
        }
    }
}