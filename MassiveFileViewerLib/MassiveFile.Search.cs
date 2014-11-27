using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace MassiveFileViewerLib
{
    public partial class MassiveFile
    {
        public async Task SearchRecordsAsync(ITargetBlock<IList<Record>> searchResultBuffer, IRecordSearch filter, int recordBatchSize, long maxSearchResults, CancellationToken ct)
        {
            var allRecordsBuffer = new BufferBlock<IList<Record>>();

            var producerCancellationSource = new CancellationTokenSource();
            var pcts = CancellationTokenSource.CreateLinkedTokenSource(producerCancellationSource.Token, ct);

            //Spawn generation of records in to seperate thread
            var producerTask = new Task(async () => await AllRecordsProducerAsync(allRecordsBuffer, recordBatchSize, pcts.Token)//.SetFault(allRecordsBuffer)
                , TaskCreationOptions.LongRunning);
            producerTask.Start();

            //Go through each record and populate output with records that pass filter
            long recordCount = 0, searchResultCount = 0;
            while (searchResultCount < maxSearchResults && !ct.IsCancellationRequested && await allRecordsBuffer.OutputAvailableAsync(ct))
            {
                IList<Record> records;

                //Keeo trieng to recieve from buffer until buffer gets empty in which
                //case we will wait for output again in outer loop
                while (searchResultCount < maxSearchResults && !ct.IsCancellationRequested && allRecordsBuffer.TryReceive(out records))
                {
                    foreach (var record in records)
                    {
                        recordCount++;

                        if (filter.IsPasses(record))
                        {
                            await searchResultBuffer.SendAsync(new Record[] { record }, ct);
                            searchResultCount++;
                        }
                        else
                        {
                            //Keep sending progress if we don't find too many results
                            if (recordCount % (this.PageSize * 1000) == 0)
                            {
                                var searchResult = new Record() { IsProgressReport = true, RecordIndex = recordCount };
                                await searchResultBuffer.SendAsync(new Record[] { searchResult }, ct);
                            }
                        }
                    }
                }
            }

            if (!producerTask.IsCompleted)
                producerCancellationSource.Cancel();

            searchResultBuffer.Complete();
        }

        private async Task AllRecordsProducerAsync(ITargetBlock<IList<Record>> allRecordsBuffer, int recordBatchSize, CancellationToken ct)
        {
            var pageIndex = 0;
            while (!ct.IsCancellationRequested)
            {
                await this.GetRecordsAsync(pageIndex, allRecordsBuffer, recordBatchSize, ct, long.MaxValue);
                if (this.EndOfFile)
                    break;
                else
                    pageIndex++;
            }

            allRecordsBuffer.Complete();
        }

    }
}
