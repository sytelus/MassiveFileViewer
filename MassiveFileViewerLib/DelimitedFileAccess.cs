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
        private int bufferCurrent;

        //TODO: Auto-detect delimiters, file encoding
        public DelimitedFileAccess(string filePath, string recordDelimiter = "\r\n", int? bufferSize = null)
            : base(filePath, bufferSize)
        {
            this.recordDelimiterBytes = Encoding.UTF8.GetBytes(recordDelimiter);
            this.bufferCurrent = 0;
        }

        public async Task ReadRecordsAsync(ITargetBlock<IList<Record>> recordsBuffer, CancellationToken ct, int recordBatchSize, long maxRecords
            , Func<Record, int, bool> onRecordCreated = null)
        {
            //Do not exit from this method prematuarely as last line needs to be executed on exit

            var recordBytes = new List<byte>();
            long recordCount = 0;
            int recordDelimiterByteIndex = 0;
            var recordBatch = new List<Record>(recordBatchSize);

            while (!ct.IsCancellationRequested && maxRecords > recordCount)
            {
                if (this.bufferCurrent >= this.BufferLength)
                {
                    await this.ReadAsync(); //read the buffer
                    this.bufferCurrent = 0;

                    if (this.BufferLength == 0)
                        break;
                }

                while (bufferCurrent < this.BufferLength)
                {
                    var thisByte = this.Buffer[bufferCurrent++];

                    //Skip UTF-8 header
                    if (thisByte == 191 && recordBytes.Count == 2 && recordBytes[0] == 239 && recordBytes[1] == 187)
                    {
                        recordBytes.RemoveRange(0, 2);
                        continue;
                    }

                    recordBytes.Add((byte)thisByte);

                    if (this.recordDelimiterBytes[recordDelimiterByteIndex] == thisByte)
                    {
                        recordDelimiterByteIndex++;

                        //Detect match with delimiter and trigger completion of record
                        if (recordDelimiterByteIndex >= this.recordDelimiterBytes.Length)
                        {
                            var record = CreateRecord(recordBytes, recordCount, recordBytes.Count - this.recordDelimiterBytes.Length, onRecordCreated);
                            if (record != null)
                            {
                                recordCount++;
                                recordBatch.Add(record);

                                if (recordBatch.Count == recordBatchSize)
                                {
                                    await recordsBuffer.SendAsync(recordBatch.ToArray(), ct);
                                    recordBatch.Clear();
                                }
                            }

                            recordBytes.Clear();
                            recordDelimiterByteIndex = 0;

                            if (maxRecords <= recordCount || ct.IsCancellationRequested)
                                break;
                        }
                    }
                    else if (recordDelimiterByteIndex > 0)
                        recordDelimiterByteIndex = 0;   //False trigger so reset delimiter search index
                }
            }

            //Compelete any pending record
            var lastRecord = CreateRecord(recordBytes, recordCount, recordBytes.Count, onRecordCreated);
            if (lastRecord != null)

                recordBatch.Add(lastRecord);
            if (recordBatch.Count > 0) 
                await recordsBuffer.SendAsync(recordBatch.ToArray(), ct);

            recordsBuffer.Complete();
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
