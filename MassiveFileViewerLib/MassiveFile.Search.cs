using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MassiveFileViewerLib
{
    public partial class MassiveFile
    {
        public void SearchRecords(BlockingCollection<IList<Record>> searchResultBuffer, IRecordSearch filter, int recordBatchSize, long maxSearchResults, CancellationToken ct)
        {
            var allRecordsBuffer = new BlockingCollection<IList<Record>>();

            //Create new cancellation token that we can use to cancel producer tasks if we got enough results
            var producerCancellationSource = new CancellationTokenSource();
            var pcts = CancellationTokenSource.CreateLinkedTokenSource(producerCancellationSource.Token, ct);

            //Spawn generation of records in to seperate thread
            var producerTask = new Task(() => AllRecordsProducer(allRecordsBuffer, recordBatchSize, pcts.Token)
                , TaskCreationOptions.LongRunning);
            producerTask.Start();

            //Start consumer task to go through producer records
            var consumerTask = new Task(() => AllRecordsConsumer(searchResultBuffer, filter, maxSearchResults, ct, allRecordsBuffer)
                , TaskCreationOptions.LongRunning); ;
            consumerTask.Start();

            //Wait till we have found enough results or search is cancelled or no more records
            consumerTask.Wait(ct);

            //Cancel producer task if needed
            if (!producerTask.IsCompleted)
                producerCancellationSource.Cancel();

            //Signal others that we are done with seatch
            searchResultBuffer.CompleteAdding();
        }

        long searchResultCount = 0;
        private void AllRecordsConsumer(BlockingCollection<IList<Record>> searchResultBuffer, IRecordSearch filter, long maxSearchResults,
            CancellationToken ct, BlockingCollection<IList<Record>> allRecordsBuffer)
        {
            //Go through each record and populate output with records that pass filter
            long recordCount = 0;

            var filterTasks = new List<Task>();

            //Keeo trieng to recieve from buffer until buffer gets empty in which
            //case we will wait for output again in outer loop
            while (Interlocked.Read(ref this.searchResultCount) < maxSearchResults && !ct.IsCancellationRequested && !allRecordsBuffer.IsCompleted)
            {
                var records = allRecordsBuffer.Take(ct);

                var filterTask = new Task(() => FilterRecords(searchResultBuffer, filter, ct, records), ct, TaskCreationOptions.None);
                filterTask.Start();

                filterTasks.Add(filterTask);

                recordCount += records.Count;

                //Keep sending progress if we don't find too many results
                if (recordCount % (this.PageSize * 1000) == 0)
                {
                    var searchResult = new Record() { IsProgressReport = true, RecordIndex = recordCount };
                    searchResultBuffer.Add(new Record[] { searchResult }, ct);
                }
            }

            Task.WaitAll(filterTasks.ToArray(), ct);
        }

        private void FilterRecords(BlockingCollection<IList<Record>> searchResultBuffer, IRecordSearch filter, CancellationToken ct,
            IEnumerable<Record> records)
        {
            foreach (var record in records.Where(r => filter.IsPasses(r)))
            {
                searchResultBuffer.Add(new Record[] {record}, ct);
                Interlocked.Increment(ref this.searchResultCount);
                //TODO: stop search if max records is already more
            }
        }

        private void AllRecordsProducer(BlockingCollection<IList<Record>> allRecordsBuffer, int recordBatchSize, CancellationToken ct)
        {
            var pageIndex = 0;
            while (!ct.IsCancellationRequested)
            {
                this.GetRecords(pageIndex, allRecordsBuffer, recordBatchSize, ct, long.MaxValue);
                if (this.EndOfFile)
                    break;
                else
                    pageIndex++;
            }

            allRecordsBuffer.CompleteAdding();
        }

    }
}
