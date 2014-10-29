using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassiveFileViewer
{
    public class ColumnRecordSearch : IRecordSearch
    {
        private string query;

        public ColumnRecordSearch(string query)
        {
            this.query = query;
        }

        public bool IsPasses(string[] columns)
        {
            return columns.Any(c => c.IndexOf(this.query, StringComparison.CurrentCultureIgnoreCase) > -1);
        }
    }
}
