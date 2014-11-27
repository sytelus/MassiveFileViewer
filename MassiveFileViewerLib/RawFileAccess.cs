using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonUtils;

namespace MassiveFileViewerLib
{
    public class RawFileAccess : IDisposable
    {
        private FileStream fileStream;
        private readonly byte[] buffer;
        private int bufferLength;

        public RawFileAccess(string filePath, int? bufferSize = null)
        {
            this.fileStream = File.OpenRead(filePath);
            this.buffer = new byte[bufferSize ?? 1 << 24];
            this.bufferLength = 0;
        }

        protected async Task ReadAsync()
        {
            this.bufferLength = await fileStream.ReadAsync(this.buffer, 0, this.buffer.Length);
        }

        protected byte[] Buffer
        {
            get { return this.buffer; }
        }

        protected int BufferLength
        {
            get { return this.bufferLength; }
        }

        public virtual bool EOF
        {
            get { return this.FileStreamPosition >= this.FileSize; }
        }

        protected long FileStreamPosition
        {
            get { return this.fileStream.Position; }
        }

        public long FileSize
        {
            get { return this.fileStream.Length; }
        }

        public virtual long Seek(long position)
        {
            if (position == this.FileStreamPosition)
                return position;
            else if (position < 0 || position >= this.FileSize)
                throw new IndexOutOfRangeException("Position {0} falls beyond valid position in file of size {1}".FormatEx(position, this.FileSize));

            this.bufferLength = 0;

            return this.fileStream.Seek(position, SeekOrigin.Begin);
        }

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
            if (this.fileStream != null)
            {
                this.fileStream.Dispose();
                this.fileStream = null;
            }
        }
        #endregion
    }
}
