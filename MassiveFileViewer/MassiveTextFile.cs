using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CommonUtils;
using System.Threading;

namespace MassiveFileViewer
{
    /// <summary>
    /// Class to load arbitrary large files of records that are delimited and browse through it in terms of pages of records
    /// </summary>
    public class MassiveTextFile : IDisposable
    {
        FileStream fileStream;

        //Calculate average line size
        private OnlineAverage averageLineSize;

        //Events when page is changed
        public delegate void OnPageRefreshedDelegate(object sender, EventArgs args);
        public event OnPageRefreshedDelegate OnPageRefreshed;

        //Save page positions so we can go back and forth
        private IDictionary<long, long> exactPagePositions;
        private IDictionary<long, long> approximatePagePositions;
        private HashSet<long> vistedPages;

        public long FileSize { get; private set; }
        private int pageSize = 1000;    //Page has 1000 lines
        const int BufferSize = 1 << 20;  //Buffer size for direct file I/O
        byte[] buffer;

        public void Load(string filePath)
        {
            DisposeFileStream();    //Recreate file stream on load
            this.ResetPagePositionsCache();

            this.fileStream = File.OpenRead(filePath);
            this.FileSize = this.fileStream.Length;
            this.buffer = new byte[BufferSize];

            this.CurrentPageIndex = 0;
        }

        private void ResetPagePositionsCache()
        {
            this.exactPagePositions = new Dictionary<long, long>() { { 0, 0 } };
            this.approximatePagePositions = new Dictionary<long, long>();
            this.vistedPages = new HashSet<long>();
            this.averageLineSize = new OnlineAverage();
        }

        /// <summary>
        /// Non-async version of refresh page
        /// </summary>
        public void RefreshCurrentPage()
        {
            Task.Run(() => this.RefreshCurrentPageAsync()).Wait();

            if (this.OnPageRefreshed != null)
                OnPageRefreshed(this, null);
        }

        /// <summary>
        /// Async version of refresh page
        /// </summary>
        private async Task RefreshCurrentPageAsync()
        {
            //If we haven't seen this page before than we would include in the stats
            var isUpdateLineSizeStats = this.vistedPages.Add(this.CurrentPageIndex);

            //Go to byte position where line starts
            await this.SeekToPageAsync(this.CurrentPageIndex);

            //Fill up the lines until we have page full or run out of file
            lines.Clear();
            while(!this.EndOfFile && lines.Count < this.PageSize)
            {
                var line = await this.ReadLineAsync();
                lines.Add(GetColumns(line));

                if (isUpdateLineSizeStats)
                    this.averageLineSize.Observe(line.Length);
            }

            UpdateNextPagePosition(this.CurrentPageIndex, this.CurrentBytePosition);
        }

        private static string[] GetColumns(string line)
        {
            return line.Split(Utils.TabDelimiter);
        }

        public bool EndOfFile
        {
            get { return this.CurrentBytePosition >= this.fileStream.Length; }
        }
        public bool StartOfFile
        {
            get { return this.CurrentBytePosition <= 0; }
        }


        public class SearchResult
        {
            public string Line { get; set; }
            public string[] Columns { get; set; }
            public long LineIndex { get; set; }
            public bool IsProgressReport { get; set; }
        }
        public async Task SearchRecordsAsync(ITargetBlock<SearchResult> filteredRecordsBuffer, IRecordSearch filter, CancellationToken ct)
        {
            var allRecordsBuffer = new BufferBlock<SearchResult>();
            AllRecordsProducerAsync(allRecordsBuffer, ct)
                .SetFault(allRecordsBuffer)
                .Forget();

            while(!ct.IsCancellationRequested && await allRecordsBuffer.OutputAvailableAsync())
            {
                SearchResult record;
                while (!ct.IsCancellationRequested && allRecordsBuffer.TryReceive(out record))
                {
                    var columns = GetColumns(record.Line);
                    if (filter.IsPasses(columns))
                    {
                        record.Columns = columns;
                        await filteredRecordsBuffer.SendAsync(record);
                    }
                    else
                    {
                        if (record.LineIndex % this.PageSize == 0)
                        {
                            record.IsProgressReport = true;
                            await filteredRecordsBuffer.SendAsync(record);
                        }
                    }
                }
            }

            filteredRecordsBuffer.Complete();
        }

        private async Task AllRecordsProducerAsync(ITargetBlock<SearchResult> allRecordsBuffer, CancellationToken ct)
        {
            var recordCount = 0;
            while (!ct.IsCancellationRequested && !this.EndOfFile)
            {
                var record = await this.ReadLineAsync();
                await allRecordsBuffer.SendAsync(new SearchResult() {Line = record, LineIndex = recordCount++});
            }

            allRecordsBuffer.Complete();
        }

        private void UpdateNextPagePosition(long currentPageIndex, long nextPageFilePosition)
        {
            if (this.IsPageApproximate(currentPageIndex))
                this.approximatePagePositions[currentPageIndex + 1] = nextPageFilePosition;
            else
                this.exactPagePositions[currentPageIndex + 1] = nextPageFilePosition;
        }
        
        public bool IsPageApproximate(long pageIndex)
        {
            return !this.exactPagePositions.ContainsKey(pageIndex);
        }

        public bool HasPagePositionCached(long pageIndex)
        {
            return this.exactPagePositions.ContainsKey(pageIndex) ||
                this.approximatePagePositions.ContainsKey(pageIndex);
        }

        private async Task SeekToPageAsync(long pageIndex)
        {
            bool isApproximateSeek = this.IsPageApproximate(pageIndex);
            long byteIndex;
            if (isApproximateSeek)
            {
                byteIndex = this.approximatePagePositions.GetValueOrDefault(pageIndex, -1);

                if (byteIndex == -1)
                    byteIndex = (long)(pageIndex * this.PageSize * this.GetAverageLineSize()) - 1;
            }
            else
                byteIndex = this.exactPagePositions[pageIndex];

            SeekToPosition(byteIndex);

            //Complete current partial line
            if (!this.HasPagePositionCached(pageIndex))
                await ReadLineAsync();

            if (isApproximateSeek)
                this.approximatePagePositions[pageIndex] = this.CurrentBytePosition;
            else
                this.exactPagePositions[pageIndex] = this.CurrentBytePosition;
        }

        private void SeekToPosition(long byteIndex)
        {
            byteIndex = Math.Min(byteIndex, this.FileSize - 1);
            byteIndex = Math.Max(byteIndex, 0);
            this.fileStream.Seek(byteIndex, SeekOrigin.Begin);
        }

        /// <summary>
        /// This method will start reading the line until LR/CF are found. After LR/CF are found it will continue
        /// reading until non-LR-CF chars are found
        /// </summary>
        private async Task<string> ReadLineAsync()
        {
            var lineBytes = new List<byte>();
            bool lineDelimiterFound = false;
            bool lineStartFound = false;
            while (!this.EndOfFile && !lineStartFound)
            {
                var bytesRead = await this.fileStream.ReadAsync(this.buffer, 0, this.buffer.Length);
                for (var i = 0; i < bytesRead; i++)
                {
                    var nextByte = this.buffer[i];
                    if (!lineDelimiterFound)
                        lineDelimiterFound = nextByte == 10 || nextByte == 13;
                    else
                        lineStartFound = nextByte != 10 && nextByte != 13;

                    if (!lineStartFound)
                        lineBytes.Add(nextByte);
                    else //we have crossed the delimiter and found the start of the first line, so reposition next seek here
                    {
                        this.fileStream.Seek(i - bytesRead, SeekOrigin.Current);
                        break;
                    }
                }
            }

            return Encoding.UTF8.GetString(lineBytes.ToArray());
        }

        private double GetAverageLineSize()
        {
            var averageLineSize = this.averageLineSize.Mean;
            if (double.IsNaN(averageLineSize) || averageLineSize <= 0)
                averageLineSize = 80;
            return averageLineSize;
        }

        //Lines for current page
        private List<string[]> lines = new List<string[]>();
        public IList<string[]> Lines
        {
            get { return this.lines; }
        }


        public int PageSize
        {
            get { return this.pageSize; }
            set 
            {
                this.ResetPagePositionsCache();
                var pageIndexMultiplier = this.pageSize / (double)value;
                this.pageSize = value;
                this.CurrentPageIndex = (long) (this.CurrentPageIndex * pageIndexMultiplier);
            }
        }

        private long currentPageIndex;
        public long CurrentPageIndex
        {
            get { return this.currentPageIndex; }
            set 
            {
                if ((value > this.currentPageIndex && this.EndOfFile) ||
                    (value < this.currentPageIndex && this.StartOfFile) ||
                    (value < 0))
                    return;

                this.currentPageIndex = value;
                this.RefreshCurrentPage();
            }
        }

        public long CurrentBytePosition
        {
            get { return this.fileStream.Position; }
        }

        public long TotalLinesEstimate
        {
            get
            {
                var averageLineSize = GetAverageLineSize();
                return (long)(this.FileSize / averageLineSize);
            }
        }
        private double GetInvStandardDeviation()
        {
            var sd = this.averageLineSize.GetStandardDeviation();
            var avg = this.GetAverageLineSize(); 
            var invSd = sd / (avg*avg - sd*sd);

            return invSd;
        }
        public long TotalLinesStandardDeviation
        {
            get
            {
                return (long)(this.FileSize * this.GetInvStandardDeviation());
            }
        }
        public long CurrentLineEstimate
        {
            get
            {
                var averageLineSize = GetAverageLineSize();
                return (long)(this.CurrentBytePosition / averageLineSize);
            }
        }
        public long TotalPagesEstimate
        {
            get
            {
                var averageLineSize = GetAverageLineSize();
                return (long)(this.FileSize / (averageLineSize * this.PageSize));
            }
        }
        public long TotalPagesStandardDeviation
        {
            get
            {
                return (long)(this.FileSize / this.PageSize * this.GetInvStandardDeviation());
            }
        }
        public long CurrentPageEstimate
        {
            get
            {
                var averageLineSize = GetAverageLineSize();
                return (long)(this.CurrentBytePosition / (averageLineSize * this.PageSize));
            }
        }

        #region Dispose pattern
        private bool disposed = false; // to detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    this.DisposeFileStream();

                    if (this.OnPageRefreshed != null)
                        this.OnPageRefreshed = null;
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void DisposeFileStream()
        {
            if (this.fileStream != null)
            {
                this.fileStream.Dispose();
                this.fileStream = null;
            }
        }
        #endregion
    }
}
