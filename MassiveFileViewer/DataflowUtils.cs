using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace MassiveFileViewer
{
    public static class DataflowUtils
    {
        public static Task SetFault(this Task task, IDataflowBlock block)
        {
            return task.ContinueWith(t => block.Fault(t.Exception), TaskContinuationOptions.NotOnRanToCompletion);
        }
    }
}
