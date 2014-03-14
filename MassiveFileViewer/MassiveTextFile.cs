using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonUtils;

namespace MassiveFileViewer
{
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
        const int BufferSize = 2 ^ 17;  //How much data to read at a time

        public void Load(string filePath)
        {
            DisposeFileStream();
            this.ResetPagePositionsCache();

            this.fileStream = File.OpenRead(filePath);
            this.FileSize = this.fileStream.Length;

            this.CurrentPageIndex = 0;
        }

        private void ResetPagePositionsCache()
        {
            this.exactPagePositions = new Dictionary<long, long>() { { 0, 0 } };
            this.approximatePagePositions = new Dictionary<long, long>();
            this.vistedPages = new HashSet<long>();
            this.averageLineSize = new OnlineAverage();
        }

        public void RefreshCurrentPage()
        {
            var updateLineSizeStats = this.vistedPages.Add(this.CurrentPageIndex);

            this.SeekToPage(this.CurrentPageIndex);

            var buffer = new byte[BufferSize];
            lines.Clear();

            while(!this.EndOfFile && lines.Count < this.PageSize)
            {
                var lineBytes = this.ReadLine();
                var line = Encoding.UTF8.GetString(lineBytes.ToArray());
                lines.Add(line);

                if (updateLineSizeStats)
                    this.averageLineSize.Observe(line.Length);
            }

            UpdateNextPagePosition(this.CurrentPageIndex, this.CurrentBytePosition);

            if (this.OnPageRefreshed != null)
                OnPageRefreshed(this, null);
        }

        public bool EndOfFile
        {
            get { return this.CurrentBytePosition >= this.fileStream.Length; }
        }
        public bool StartOfFile
        {
            get { return this.CurrentBytePosition <= 0; }
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

        private void SeekToPage(long pageIndex)
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
                ReadLine().ToVoid();

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

        private IEnumerable<byte> ReadLine()
        {
            bool lineDelimiterFound = false;
            bool lineStartFound = false;
            while (!this.EndOfFile && !lineStartFound)
            {
                var nextByte = this.fileStream.ReadByte();

                if (!lineDelimiterFound)
                    lineDelimiterFound = nextByte == 10 || nextByte == 13;
                else
                    lineStartFound = nextByte != 10 && nextByte != 13;

                if (!lineStartFound)
                    yield return (byte)nextByte;
            }

            //Reposition at the start of next line
            if (lineStartFound && this.CurrentBytePosition > 0)
                this.fileStream.Seek(-1, SeekOrigin.Current);
        }

        private double GetAverageLineSize()
        {
            var averageLineSize = this.averageLineSize.Mean;
            if (double.IsNaN(averageLineSize) || averageLineSize <= 0)
                averageLineSize = 80;
            return averageLineSize;
        }

        //Lines for current page
        private List<string> lines = new List<string>();
        public IList<string> Lines
        {
            get { return this.lines; }
        }


        private int pageSize = 1000;
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
