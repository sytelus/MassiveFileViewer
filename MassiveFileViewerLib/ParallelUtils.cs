using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace MassiveFileViewerLib
{
    public static class ParallelUtils
    {
        public static Task SetFault(this Task task, IDataflowBlock block)
        {
            return task.ContinueWith(t => block.Fault(t.Exception), TaskContinuationOptions.NotOnRanToCompletion);
        }
    }
}
