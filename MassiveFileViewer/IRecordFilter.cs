using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MassiveFileViewer
{
    public interface IRecordSearch
    {
        bool IsPasses(string[] line);
    }
}
