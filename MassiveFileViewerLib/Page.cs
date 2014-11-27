using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassiveFileViewerLib
{
    public class Page
    {
        public IList<Record> Records { get; set; }
        public long StartByteIndex { get; set; }
        public long EndByteIndex { get; set; }
    }
}