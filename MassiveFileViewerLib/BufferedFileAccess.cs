using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonUtils;

namespace MassiveFileViewerLib
{
    public class BufferedFileAccess : IDisposable
    {
        FileStream fileStream;
        public long FileSize { get; private set; }
        private readonly int bufferSize;  //Buffer size for direct file I/O
        readonly byte[] buffer;
        int bufferEnd;
        int bufferCurrent;

        public BufferedFileAccess(string filePath, int bufferSize = 1 << 26)
        {
            this.bufferSize = bufferSize;
            this.fileStream = File.OpenRead(filePath);
            this.FileSize = this.fileStream.Length;
            this.buffer = new byte[bufferSize];
            ResetBuffer();
        }

        private void ResetBuffer()
        {
            this.bufferEnd = -1;
            this.bufferCurrent = -1;
        }

        public bool EndOfFile
        {
            get { return this.CurrentBytePosition > this.fileStream.Length; }
        }

        public int Current
        {
            get
            {
                if (this.bufferCurrent < 0 || this.bufferCurrent > this.bufferEnd)
                    return -1;
                else 
                    return this.buffer[this.bufferCurrent];
            }
        }

        public async Task SeekAsync(long position, CancellationToken ct)
        {
            if (position < 0 || position > this.FileSize - 1)
                throw new IndexOutOfRangeException("Position must be seek between {0} and {1} inclusive".FormatEx(0, this.FileSize - 1));

            //if buffer is not full OR we are seeking before buffer OR we are seeking at or after current position
            if (this.bufferEnd < 0 || position < (this.fileStream.Position - this.bufferEnd - 1) || position >= this.fileStream.Position)
            {
                FileStreamSeek(position);

                ResetBuffer();
                await NextAsync(ct);
            }
            else
            {
                this.bufferCurrent = (int) (this.bufferEnd - (this.fileStream.Position - position) + 1);
                Debug.Assert(this.bufferCurrent >= 0 && this.bufferCurrent <= this.bufferEnd, 
                    "this.bufferCurrent is out of range {0} to {1} when seeking to {2} from {3}".FormatEx(0, this.bufferEnd, position, this.fileStream.Position));
            }
        }

        private void FileStreamSeek(long position)
        {
            if (position != this.fileStream.Position)
                this.fileStream.Seek(position, SeekOrigin.Begin);
        }

        public async Task NextAsync(CancellationToken ct)
        {
                if (this.bufferCurrent >= this.bufferEnd)
                {
                    if (!this.EndOfFile)
                    {
                        //Cancellation tokem not passed because it would leave buffer corrupted
                        this.bufferEnd = await this.fileStream.ReadAsync(this.buffer, 0, this.buffer.Length) - 1;
                        this.bufferCurrent = this.bufferEnd >= 0 ? 0 : -1;
                    }
                    else
                        this.bufferCurrent = this.bufferEnd + 1;
                }
                else
                    this.bufferCurrent++;
        }

        public async Task PreviousAsync(CancellationToken ct)
        {
            if (this.bufferCurrent <= 0)
            {
                var seekPosition = this.fileStream.Position - (this.bufferEnd + 1 + bufferSize);
                var bytesToRead = this.fileStream.Position - this.bufferSize;
                if (bytesToRead > 0)
                {
                    seekPosition = Math.Max(seekPosition, 0);
                    bytesToRead = Math.Min(bytesToRead, this.bufferSize);

                    FileStreamSeek(seekPosition);

                    //Cancellation tokem not passed because it would leave buffer corrupted
                    this.bufferEnd = await this.fileStream.ReadAsync(this.buffer, 0, (int) bytesToRead) - 1;
                    this.bufferCurrent = this.bufferEnd;
                }
                else
                    this.bufferCurrent = -1;
            }
            else
                this.bufferCurrent--;
        }

        public long CurrentBytePosition
        {
            get
            {
                return this.fileStream.Position - (this.bufferEnd - this.bufferCurrent) - 1;
            }
        }

        #region Dispose pattern
        private bool disposed = false; // to detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                    this.DisposeFileStream();

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
