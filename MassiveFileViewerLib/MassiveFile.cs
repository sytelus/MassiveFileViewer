using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonUtils;
using System.Threading;
using System.Diagnostics;

namespace MassiveFileViewerLib
{
    /// <summary>
    /// Class to load arbitrary large files of records that are delimited and browse through it in terms of pages of records
    /// </summary>
    public partial class MassiveFile : IDisposable
    {
        private DelimitedFileAccess fileAccess;
        private PagePositions pagePositions;

        public MassiveFile(string filePath, int pageSize = 1000, int? fileBufferSize = null)
        {
            this.fileAccess = new DelimitedFileAccess(filePath, bufferSize: fileBufferSize);
            this.pagePositions = new PagePositions(pageSize, this.fileAccess.FileSize);
        }

        public bool IsPageApproximate(long pageIndex)
        {
            return this.pagePositions.IsPageApproximate(pageIndex);
        }

        /// <summary>
        /// Go to specified page. If page start was not known then go to approximate position and look for next record start.
        /// This method should only be called as part of GetRecordsAsync().
        /// </summary>
        private void SeekToPage(long pageIndex, CancellationToken ct)
        {
            if (pageIndex < 0)
                throw new IndexOutOfRangeException("Page index {0} is not valid because it is before or beyond the file".FormatEx(pageIndex));

            //Seek to byte position
            var byteIndex = this.pagePositions.GetPageByteStart(pageIndex);
            this.fileAccess.Seek(byteIndex);

            //Complete current partial record if we are visiting this page for the first time
            if (!this.pagePositions.IsPagePositionCached(pageIndex))
                this.fileAccess.ReadRecords(null, ct, 1, 1);
        }

        public void GetRecords(long pageIndex, BlockingCollection<IList<Record>> recordsBuffer, int recordBatchSize, CancellationToken ct)
        {
            GetRecords(pageIndex, recordsBuffer, recordBatchSize, ct, this.PageSize);
        }

        public void GetRecords(long pageIndex, BlockingCollection<IList<Record>> recordsBuffer, int recordBatchSize, CancellationToken ct, long maxRecords)
        {
            SeekToPage(pageIndex, ct);

            //If we haven't seen this page before than we would include in the stats
            var pageStartBytePosition = this.CurrentBytePosition;

            //Fill up the records until we have page full or run out of file
            this.fileAccess.ReadRecords(recordsBuffer, ct, recordBatchSize, maxRecords,
                this.pagePositions.IsPagePositionCached(pageIndex) ? this.ObserveRecord : (Func<Record, int, bool>) null);

            //record the stats for this page
            this.pagePositions.SetPageExtents(pageIndex, pageStartBytePosition, this.CurrentBytePosition - 1);
        }
        
        private bool ObserveRecord(Record record, int recordByteSize)
        {
            this.pagePositions.ObserveRecord(recordByteSize);
            return false;   //Indicates record is not invalid when used in onRecordCreated event
        }

        public int PageSize
        {
            get { return this.pagePositions.PageSize; }
        }

        public double ResetPageSize(int pageSize, CancellationToken ct)
        {
            //Recalculate current page index and then change page size
            var oldPageSize = this.pagePositions.PageSize;
            this.pagePositions = new PagePositions(pageSize, this.fileAccess.FileSize);
            var pageIndexMultiplier = oldPageSize / (double)pageSize;

            //Seek factor for page index adjustment after resize
            return pageIndexMultiplier;
        }

        public long CurrentBytePosition
        {
            get { return this.fileAccess.CurrentBytePosition; }
        }

        public bool EndOfFile
        {
            get { return this.fileAccess.EOF; }
        }
        public bool StartOfFile
        {
            get { return this.CurrentBytePosition <= 0; }
        }
        public long FileSize
        {
            get { return this.fileAccess.FileSize; }
        }

        #region Page stats
        public long TotalRecordsEstimate
        {
            get
            {
                var averageRecordSize = this.pagePositions.GetAverageRecordSize();
                return (long)(this.FileSize / averageRecordSize);
            }
        }
        public long TotalRecordsStandardDeviation
        {
            get
            {
                return (long)(this.FileSize * this.pagePositions.GetInvStandardDeviation());
            }
        }
        public long CurrentRecordEstimate
        {
            get
            {
                var averageRecordSize = this.pagePositions.GetAverageRecordSize();
                return (long)(this.CurrentBytePosition / averageRecordSize);
            }
        }
        public long TotalPagesEstimate
        {
            get
            {
                var averageRecordSize = this.pagePositions.GetAverageRecordSize();
                return (long)(this.FileSize / (averageRecordSize * this.PageSize));
            }
        }
        public long TotalPagesStandardDeviation
        {
            get
            {
                return (long) (this.FileSize/(double) this.PageSize * this.pagePositions.GetInvStandardDeviation());
            }
        }
        public long CurrentPageEstimate
        {
            get
            {
                var averageRecordSize = this.pagePositions.GetAverageRecordSize();
                return (long)(this.CurrentBytePosition / (averageRecordSize * this.PageSize));
            }
        }
        #endregion

        #region Dispose pattern
        private bool disposed = false; // to detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    this.DisposeResources();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void DisposeResources()
        {
            if (this.fileAccess != null)
            {
                this.fileAccess.Dispose();
                this.fileAccess = null;
            }
        }
        #endregion
    }
}
