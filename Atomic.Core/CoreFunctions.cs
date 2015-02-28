
namespace Atomic.Core
{
    public static class CoreFunctions
    {
        static public bool DefaultMetFunction()
        {
            return false;
        }
                
        static public bool DefaultMetFunction(IRunnable task)
        {
            return false;
        }
        
        static public bool DefaultMetFunction(IValue value)
        {
            return false;
        }

        static public bool DefaultMetFunction(ICondition condition)
        {
            return false;
        }

        static public bool DefaultMetFunction(ICondition[] conditions)
        {
            return false;
        }

        static public void DefaultRunFunction(IRunnable task)
        {
            // do nothing
        }

        static public bool TaskRunCompleted(IRunnable task)
        {
            return task.CurrentState == TaskState.RunComplete;
        }

        static public bool TaskDone(ITask task)
        {
            return task.CurrentState == TaskState.Done;
        }

        static public bool AllConditionsMet(ICondition[] conditions)
        {
            bool met = true;

            foreach (ICondition c in conditions)
            {
                met &= c.Met();
            }

            return met;
        }

        static public bool OneConditionMet(ICondition[] conditions)
        {
            bool met = false;
            if (conditions.Length == 0) return true;

            foreach (ICondition c in conditions)
            {
                met |= c.Met();
            }

            return met;
        }
    }
}
