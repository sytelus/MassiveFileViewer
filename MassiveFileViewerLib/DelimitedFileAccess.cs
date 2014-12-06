using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CommonUtils;

namespace MassiveFileViewerLib
{
    public class DelimitedFileAccess : RawFileAccess
    {
        private readonly byte[] delimBytes;
        private readonly string delimString;
        private int bufferCurrent;
        private string[] delimStringArray;
        private static readonly byte[] EmptyBytes = new byte[] {};

        //TODO: Auto-detect delimiters, file encoding
        public DelimitedFileAccess(string filePath, string recordDelimiter = "\r\n", int? bufferSize = null)
            : base(filePath, bufferSize)
        {
            this.delimBytes = Encoding.UTF8.GetBytes(recordDelimiter);
            this.delimString = recordDelimiter;
            this.delimStringArray = new string[] {this.delimString};
            this.bufferCurrent = 0;
        }

        public async Task ReadRecordsAsync(ITargetBlock<IList<Record>> recordsBuffer, CancellationToken ct, int recordBatchSize, long maxRecords
            , Func<Record, int, bool> onRecordCreated = null)
        {
            //Do not exit from this method prematuarely as last line needs to be executed on exit

            var pendingBytes = new List<byte>();
            long recordCount = 0;
            var recordBatch = new List<Record>(recordBatchSize);
            var delimStart = 0;

            while (!ct.IsCancellationRequested && maxRecords > recordCount)
            {
                //if we are past the buffer end, read more bytes
                if (this.bufferCurrent >= this.BufferLength)
                {
                    await this.ReadAsync(); //read the buffer
                    this.bufferCurrent = 0;

                    if (this.BufferLength == 0)
                        break;  //EOF
                }

                //until we are at the end of the buffer
                while (this.bufferCurrent < this.BufferLength)    
                {
                    var foundIndex = FindDelimiter(this.Buffer, this.BufferLength, this.bufferCurrent, this.delimBytes, ref delimStart);
                    if (foundIndex >= 0)
                    {
                        var record = CreateRecord(this.Buffer, pendingBytes, this.bufferCurrent, foundIndex, recordCount, delimBytes, onRecordCreated);
                        this.bufferCurrent = foundIndex + 1;    //Start next search afer found location

                        //Batch the record
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
                    else //we did not found delimiter
                    {
                        //Save pending bytes
                        for(var i=this.bufferCurrent; i < this.BufferLength; i++)
                            pendingBytes.Add(this.Buffer[i]);

                        this.bufferCurrent = this.BufferLength;
                    }
                }
            }

            //Compelete any pending record
            if (this.EOF && !(maxRecords <= recordCount || ct.IsCancellationRequested))
            {
                var lastRecord = CreateRecord(this.Buffer, pendingBytes, this.bufferCurrent, this.BufferLength - 1, recordCount, EmptyBytes, onRecordCreated);
                if (lastRecord != null)
                    recordBatch.Add(lastRecord);
            }
            //else cancellation requested or we reached max records

            //Flush record batch
            if (recordBatch.Count > 0) 
                await recordsBuffer.SendAsync(recordBatch.ToArray(), ct);

            recordsBuffer.Complete();
        }

        private static int FindDelimiter(byte[] buffer, int bufferLength, int bufferCurrent, byte[] delimBytes, ref int delimStart)
        {
            //Reason this code is not so simple is because we may start anywhere in buffer and we have multiple delimiter bytes to match

            while (bufferCurrent < bufferLength)    //We may break out if we find delimiter match or determine there is none
            {
                int bufferIndex, delimIndex;    //two indices to compare arrays

                //If we are looking from start of delimiter
                if (delimStart == 0)
                {
                    //Search for the first byte
                    var foundIndex = Array.IndexOf(buffer, delimBytes[0], bufferCurrent);

                    //if first byte not found then we don't need to compare rest of delim array
                    if (foundIndex < 0)
                        break;
                    else //Compare rest of the delim array from next position
                    {
                        bufferIndex = foundIndex + 1;
                        delimIndex = delimStart + 1;
                    }
                }
                else //We have already compared previous bytes in delimiter so now compare next bytes
                {
                    bufferIndex = bufferCurrent;
                    delimIndex = delimStart;
                }
                
                //Match rest of the delimiter array
                for (; bufferIndex < bufferLength && delimIndex < delimBytes.Length && delimBytes[delimIndex] == buffer[bufferIndex]; bufferIndex++, delimIndex++)
                { }

                //Case 1: All delim bytes found
                if (delimIndex == delimBytes.Length)
                {
                    delimStart = 0; //Next search for delim starts from byte 0
                    return bufferIndex - 1; //Return position of last delimiter byte found
                }
                //Case 2A: Delim bytes not found, buffer ened
                else if (bufferIndex >= bufferLength)
                {
                    delimStart = delimIndex; //After reloading buffer we will start from where we left off
                    return -1;
                }
                //else Case 2B: Delim bytes not found, buffer not ended
                bufferCurrent++;   //TODO: we should be starting search back at bufferCurrent - delimiterStart + 1
                delimStart = 0;
            }

            //If we are here then we have run out of buffer
            return -1;
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

        private static Record CreateRecord(byte[] buffer, IList<byte> pendingBytes, int start, int end, long recordIndex, byte[] delimBytes, Func<Record, int, bool> onRecordCreated = null)
        {
            var byteCount = end - start + 1 + pendingBytes.Count - delimBytes.Length;
            if (byteCount < 0)
                return null;

            string text;
            if (pendingBytes.Count == 0)
                text = Encoding.UTF8.GetString(buffer, start, byteCount);
            else
            {
                for(var i=start; i <= end; i++)
                    pendingBytes.Add(buffer[i]);

                text = Encoding.UTF8.GetString(pendingBytes.ToArray(), 0, byteCount);
                pendingBytes.Clear();
            }

            //Remove BOM if exists
            if (text.Length > 0 && text[0] == 65279) //Unicode 65279 == Byte order mark or BOM
                text = text.Substring(1);

            var record = new Record() {Text = text, RecordIndex = recordIndex, IsProgressReport = false};
            var isInvalidRecord = false;
            if (onRecordCreated != null)
                isInvalidRecord = onRecordCreated(record, byteCount + delimBytes.Length);

            if (!isInvalidRecord)
                return record;
            
            return null;
        }
    }
}
