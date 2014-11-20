using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MassiveFileViewerLib
{
    public interface IRecordSearch
    {
        bool IsPasses(Record record);
    }
}
