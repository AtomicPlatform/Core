
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
        
        static public bool ValueEqualFunction(IValue value, object expectedResult)
        {
            return value.Value.Equals(expectedResult);
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
            return task.CurrentState == RunState.RunComplete;
        }

        static public bool TaskDone(ITask task)
        {
            return task.CurrentState == RunState.Done;
        }
    }
}
