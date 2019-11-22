using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TPLBeestWPF.Stuff
{
    public static class ExecutionDataflowBlockOptionsCreator
    {
        public static ExecutionDataflowBlockOptions SynchronizeForUiThread(bool actuallyDoIt, ExecutionDataflowBlockOptions executionDataflowBlockOptions)
        {
            if (actuallyDoIt && SynchronizationContext.Current != null)
            {
                executionDataflowBlockOptions.TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            }
            return executionDataflowBlockOptions;
        }
    }
}
