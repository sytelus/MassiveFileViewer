using CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassiveFileViewerLib
{
    public class ColumnRecordSearch : IRecordSearch
    {
        private readonly string query;

        public ColumnRecordSearch(string query)
        {
            this.query = query;
        }

        public bool IsPasses(Record record)
        {
            var columns = record.Text.Split(Utils.TabDelimiter);
            return columns.Any(c => c.IndexOf(this.query, StringComparison.CurrentCultureIgnoreCase) > -1);
        }

        
    }
}
