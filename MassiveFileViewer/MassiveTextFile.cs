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
        private long sampleLinesSizeSum;
        private long sampleLinesCount;
        private long nextPagePosition, previousPagePosition;

        public delegate void OnPageRefreshedDelegate(object sender, EventArgs args);
        public event OnPageRefreshedDelegate OnPageRefreshed;

        private double GetAverageLineSize()
        {
            var averageLineSize = this.sampleLinesSizeSum / (double)this.sampleLinesCount;
            if (double.IsNaN(averageLineSize))
                averageLineSize = 1;
            return averageLineSize;
        }

        public long FileSize { get; private set; }
        const int BufferSize = 2 ^ 17;

        private List<string> lines = new List<string>();
        public IList<string> Lines
        {
            get { return this.lines; }
        }

        public void Load(string filePath)
        {
            if (this.fileStream != null)
            {
                this.fileStream.Dispose();
                this.fileStream = null;
            }

            this.fileStream = File.OpenRead(filePath);
            this.FileSize = this.fileStream.Length;
            this.previousPagePosition = this.fileStream.Position;
            this.nextPagePosition = this.fileStream.Position;

            this.RefreshCurrentPage(true);
        }

        public void RefreshCurrentPage(bool updateLineSizeStats = false)
        {
            var originalPosition = this.fileStream.Position;

            var buffer = new byte[BufferSize];
            lines.Clear();

            if (updateLineSizeStats)
            {
                this.sampleLinesCount = 0;
                this.sampleLinesSizeSum = 0;
            }

            while(this.fileStream.Position < this.fileStream.Length && lines.Count < this.PageSize)
            {
                var lineBytes = this.ReadLine();
                var line = Encoding.UTF8.GetString(lineBytes.ToArray());
                lines.Add(line.TrimEnd());

                if (updateLineSizeStats)
                    this.sampleLinesSizeSum += line.Length;
            }

            if (updateLineSizeStats)
                this.sampleLinesCount = lines.Count;

            nextPagePosition = this.fileStream.Position;
            originalPosition = this.SeekToPosition(originalPosition);

            if (this.OnPageRefreshed != null)
                OnPageRefreshed(this, null);
        }
        
        private void SeekToPage(long pageIndex)
        {
            var byteIndex = (long)(pageIndex * this.PageSize * this.GetAverageLineSize()) - 1;
            byteIndex = SeekToPosition(byteIndex);

            if (this.fileStream.Position > 0)
                ReadLine().ToVoid();
        }

        private long SeekToPosition(long byteIndex)
        {
            byteIndex = Math.Min(byteIndex, this.FileSize - 1);
            byteIndex = Math.Max(byteIndex, 0);
            this.fileStream.Seek(byteIndex, SeekOrigin.Begin);
            return byteIndex;
        }

        private IEnumerable<byte> ReadLine()
        {
            bool lineDelimiterFound = false;
            bool lineStartFound = false;
            while (this.fileStream.Position < this.fileStream.Length && !lineStartFound)
            {
                var nextByte = this.fileStream.ReadByte();

                if (!lineDelimiterFound)
                    lineDelimiterFound = nextByte == 10 || nextByte == 13;
                else
                    lineStartFound = nextByte != 10 && nextByte != 13;

                if (!lineStartFound)
                    yield return (byte)nextByte;
            }

            if (lineStartFound && this.fileStream.Position > 0)
                this.fileStream.Seek(-1, SeekOrigin.Current);
        }

        private int pageSize = 1000;
        public int PageSize
        {
            get { return this.pageSize; }
            set 
            { 
                this.pageSize = value;
                this.SeekToPage(this.currentPageIndex);
                this.RefreshCurrentPage();
            }
        }

        private long currentPageIndex;
        public long CurrentPageIndex
        {
            get { return this.currentPageIndex; }
            set 
            {
                if (value - this.currentPageIndex == 1)
                {
                    this.previousPagePosition = this.fileStream.Position;
                    this.SeekToPosition(this.nextPagePosition);
                }
                else if (this.currentPageIndex - value == 1)
                    this.SeekToPosition(this.previousPagePosition);
                else
                    this.SeekToPage(value);

                this.currentPageIndex = this.CurrentPageEstimate;
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
                    if (fileStream != null)
                    {
                        fileStream.Dispose();
                        fileStream = null;
                    }

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
        #endregion
    }
}
