using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace MassiveFileViewerLib
{
    public class DelimitedFileAccess : RawFileAccess
    {
        private readonly byte[] recordDelimiterBytes;
        private readonly string recordDelimiter;
        private int bufferCurrent;
        private string[] recordDelimiterAsArray;

        //TODO: Auto-detect delimiters, file encoding
        public DelimitedFileAccess(string filePath, string recordDelimiter = "\r\n", int? bufferSize = null)
            : base(filePath, bufferSize)
        {
            this.recordDelimiterBytes = Encoding.UTF8.GetBytes(recordDelimiter);
            this.recordDelimiter = recordDelimiter;
            this.recordDelimiterAsArray = new string[] {this.recordDelimiter};
            this.bufferCurrent = 0;
        }

        public async Task ReadRecordsAsync(ITargetBlock<IList<Record>> recordsBuffer, CancellationToken ct, int recordBatchSize, long maxRecords
            , Func<Record, int, bool> onRecordCreated = null)
        {
            //Do not exit from this method prematuarely as last line needs to be executed on exit

            var recordBytes = new List<byte>();
            long recordCount = 0;
            var recordBatch = new List<Record>(recordBatchSize);
            var recordDelimiterIndex = 0;

            while (!ct.IsCancellationRequested && maxRecords > recordCount)
            {
                //if we are past the buffer end, read more bytes
                if (this.bufferCurrent >= this.BufferLength)
                {
                    await this.ReadAsync(); //read the buffer
                    this.bufferCurrent = 0;

                    if (this.BufferLength == 0)
                        break;
                }

                while (this.bufferCurrent < this.BufferLength)    //until we are at the end of the buffer
                {
                    int foundIndex;
                    if (recordDelimiterIndex == 0)
                        foundIndex = Array.IndexOf(this.Buffer, this.recordDelimiterBytes[0], this.bufferCurrent);
                    else
                        foundIndex = this.Buffer[this.bufferCurrent] == this.recordDelimiterBytes[recordDelimiterIndex] ? this.bufferCurrent : -1;

                    if (foundIndex < 0)
                    {
                        recordDelimiterIndex = 0;

                        //If first delimiter byte not found then add whole buffer to pending bytes
                        this.CopyFromBuffer(recordBytes, recordDelimiterIndex == 0 ? this.BufferLength : this.bufferCurrent + 1);
                    }
                    else
                    {
                        //Look for next delimiter bytes
                        int i = foundIndex + 1;
                        recordDelimiterIndex++;
                        while (i < this.BufferLength && recordDelimiterIndex < this.recordDelimiterBytes.Length)
                        {
                            if (this.recordDelimiterBytes[recordDelimiterIndex++] != this.Buffer[i++])
                            {
                                recordDelimiterIndex = 0;
                                break;
                            }
                        }

                        //If we found all delimiter bytes
                        if (recordDelimiterIndex == this.recordDelimiterBytes.Length)
                        {
                            recordDelimiterIndex = 0;

                            this.CopyFromBuffer(recordBytes, i);

                            var record = CreateRecord(recordBytes, recordCount, recordBytes.Count - this.recordDelimiterBytes.Length, onRecordCreated);
                            recordBytes.Clear();

                            if (record != null)
                            {
                                recordCount++;
                                recordBatch.Add(record);
                                if (maxRecords <= recordCount || ct.IsCancellationRequested)
                                    break;

                                //Flush this batch of record
                                if (recordBatch.Count >= recordBatchSize)
                                {
                                    await recordsBuffer.SendAsync(recordBatch.ToArray(), ct);
                                    recordBatch.Clear();
                                }
                            }
                        }
                        else if (i == this.BufferLength) //Buffer ended but match was found
                            this.CopyFromBuffer(recordBytes, i);
                        else //match was not found
                            this.CopyFromBuffer(recordBytes, this.bufferCurrent + 1);
                    }
                }
            }

            //Compelete any pending record
            if (this.EOF)
            {
                var lastRecord = CreateRecord(recordBytes, recordCount, recordBytes.Count, onRecordCreated);
                if (lastRecord != null)
                    recordBatch.Add(lastRecord);
            }

            //Flush record batch
            if (recordBatch.Count > 0) 
                await recordsBuffer.SendAsync(recordBatch.ToArray(), ct);

            recordsBuffer.Complete();
        }
        
        private void CopyFromBuffer(IList<byte> recordBytes, int uptoIndex)
        {
            var bufferStart = this.bufferCurrent; //Add bytes for decoding from this index

            //Skip UTF-8 header
            if (bufferStart == 0 && this.BufferLength > 2 && this.Buffer[0] == 239 && this.Buffer[1] == 187 &&
                this.Buffer[2] == 191)
                bufferStart = 3;

            //Ad bytes for string conversion
            for (var i = bufferStart; i < uptoIndex; i++)
                recordBytes.Add(this.Buffer[i]);

            this.bufferCurrent = uptoIndex;
        }

        public long CurrentBytePosition
        {
            get { return this.FileStreamPosition - (this.BufferLength - this.bufferCurrent); }
        }

        public override bool EOF
        {
            get { return this.CurrentBytePosition >= this.FileSize ; }
        }

        public override long Seek(long position)
        {
            var currentMin = this.FileStreamPosition - this.BufferLength;
            var currentMax = this.FileStreamPosition - 1;

            //if new position falls within buffer then just adjust the current pointer
            if (position >= currentMin && position <= currentMax)
                this.bufferCurrent = (int) (this.BufferLength - (this.FileStreamPosition - position));
            else
            {
                base.Seek(position);

                //At this point seek may have not happened because asked position is same as current
                //In that case we move to beyond buffer. If actual seek was done then buffer should have 
                //been emtied in which case we should again to buffer length.
                this.bufferCurrent = this.BufferLength;
            }

            return this.CurrentBytePosition;
        }

        private static Record CreateRecord(List<byte> recordBytes, long recordIndex, int recordByteCount
            , Func<Record, int, bool> onRecordCreated = null)
        {
            if (recordBytes.Count > 0)
            {
                var text = Encoding.UTF8.GetString(recordBytes.ToArray(), 0, recordByteCount);
                var record = new Record() {Text = text, RecordIndex = recordIndex, IsProgressReport = false};
                var isInvalidRecord = false;
                if (onRecordCreated != null)
                    isInvalidRecord = onRecordCreated(record, recordBytes.Count);

                if (!isInvalidRecord)
                    return record;
            }

            return null;
        }
    }
}
