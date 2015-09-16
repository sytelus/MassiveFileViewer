using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using CommonUtils;
using CommonWinFormUtils;
using System.Threading;
using MassiveFileViewerLib;

namespace MassiveFileViewer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        MassiveFile massiveFile = null;
        long currentPageIndex = -1;
        //Task currentSearchTask = null;
        readonly CancellationTokenSource cts = new CancellationTokenSource();
        long pageIndexBeforeSearch;

        private void MainForm_Load(object sender, EventArgs e)
        {
            buttonLoadFile_Click(null, null);
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (massiveFile != null)
                massiveFile.Dispose();
        }

        private async void buttonLoadFile_Click(object sender, EventArgs e)
        {
            if (massiveFile != null)
                massiveFile.Dispose();

            massiveFile = new MassiveFile(textBoxFilePath.Text);
            this.currentPageIndex = -1;
            await this.GoToPageAsync(0);
        }

        private async Task GoToPageAsync(long pageIndex)
        {
            this.currentPageIndex = pageIndex;

            var approximatePrefix = massiveFile.IsPageApproximate(this.currentPageIndex) ? "~" : string.Empty;
            toolStripStatusLabelCurrentRecord.Text = "Current Record: " + approximatePrefix + massiveFile.CurrentRecordEstimate.ToString("N0");
            toolStripStatusLabelTotalRecords.Text = string.Concat("Total Records: ~", massiveFile.TotalRecordsEstimate.ToString("N0"),
                "±", ((int)massiveFile.TotalRecordsStandardDeviation).ToString("N0"));
            toolStripStatusLabelFileSize.Text = @"File Size: " + massiveFile.FileSize.ToString("N0");
            toolStripStatusLabelCurrentPosition.Text = @"Current Byte#: " + massiveFile.CurrentBytePosition.ToString("N0");
            toolStripStatusLabelCurrentPage.Text = @"Current Page:  " + approximatePrefix + this.currentPageIndex.ToString("N0");
            toolStripStatusLabelTotalPages.Text = string.Concat("Total Pages: ~", massiveFile.TotalPagesEstimate.ToString("N0"),
                "±", ((int)massiveFile.TotalPagesStandardDeviation).ToString("N0"));
            textBoxCurrentPageIndex.Text = this.currentPageIndex.ToString("N0");
            textBoxPageSize.Text = massiveFile.PageSize.ToString("N0");

            this.ClearGrid();

            var progress = PrepareUiUpdate(massiveFile.PageSize);

            await DisplayRecordsAsync(this.massiveFile, this.currentPageIndex, progress, cts.Token);
        }

        private static async Task DisplayRecordsAsync(MassiveFile massiveFile, long pageIndex, IProgress<Record> progress, CancellationToken ct)
        {
            var recordsBuffer = new BlockingCollection<IList<Record>>();
            var  recordsTask = new Task(() => massiveFile.GetRecords(pageIndex, recordsBuffer, 1, ct), ct, TaskCreationOptions.LongRunning);
            recordsTask.Start();
            await DisplayTaskResultsAsync(progress, ct, recordsBuffer);
        }

        private void ClearGrid()
        {
            dataGridViewMain.ColumnCount = 0;
            dataGridViewMain.Rows.Clear();
        }

        private void AddRecordInGrid(Record record, long rowIndex)
        {
            var columns = record.Text.Split(Utils.TabDelimiter);
            if (dataGridViewMain.ColumnCount == 0)
            {
                dataGridViewMain.ColumnCount = columns.Length;
                for (var columnIndex = 0; columnIndex < columns.Length; columnIndex++)
                    dataGridViewMain.Columns[columnIndex].Name = columnIndex.ToStringCurrentCulture();
            }

            var index = dataGridViewMain.Rows.Add(columns);
            dataGridViewMain.Rows[index].HeaderCell.Value = rowIndex.ToString();
        }

        private async void buttonNext_Click(object sender, EventArgs e)
        {
            if (!this.massiveFile.EndOfFile)
                await this.GoToPageAsync(this.currentPageIndex + 1);
        }

        private async void buttonPrevious_Click(object sender, EventArgs e)
        {
            if (this.currentPageIndex > 0)
                await this.GoToPageAsync(this.currentPageIndex - 1);
        }

        private async void buttonGotoPage_Click(object sender, EventArgs e)
        {
            await this.GoToPageAsync(int.Parse(textBoxCurrentPageIndex.Text));
        }

        private async void buttonChangePageSize_Click(object sender, EventArgs e)
        {
            var changeFactor = massiveFile.ResetPageSize(int.Parse(textBoxPageSize.Text), cts.Token);
            await this.GoToPageAsync((long)(this.currentPageIndex * changeFactor));
        }

        private Progress<Record> PrepareUiUpdate(long maxRecordsExpected)
        {
            this.progressBarSearch.Maximum = (int)maxRecordsExpected;   //TODO: handle long balues properly
            this.progressBarSearch.Minimum = 0;
            this.ClearGrid();
            var sw = Stopwatch.StartNew();
            long firstRecordIndex = -1;

            var progress = new Progress<Record>((record) =>
            {
                if (!record.IsProgressReport)
                {
                    this.AddRecordInGrid(record, record.RecordIndex);
                }

                if (firstRecordIndex == -1)
                    firstRecordIndex = record.RecordIndex;

                var recordCount = record.RecordIndex - firstRecordIndex + 1;

                this.progressBarSearch.Value = (int) recordCount;
                this.labelSearchProgress.Text = record.RecordIndex.ToString("N0");

                var throughput = sw.Elapsed.TotalMilliseconds/recordCount;  //millisecond/record
                var eta = throughput*maxRecordsExpected;
                this.labelEta.Text = @"{0} ms/record, ETA: {1}s".FormatEx(throughput.ToString("N0"),
                    (eta/1000).ToString("N0"));
            });

            return progress;
        }

        private async void buttonSearch_Click(object sender, EventArgs e)
        {
            this.pageIndexBeforeSearch = this.currentPageIndex;
            var progress = PrepareUiUpdate(massiveFile.TotalRecordsEstimate);

            await DisplaySearchResultsAsync(this.massiveFile, textBoxQuery.Text, progress, this.massiveFile.PageSize, cts);
        }

        private static async Task DisplaySearchResultsAsync(MassiveFile massiveFile, string query, IProgress<Record> progress, long maxResults, CancellationTokenSource cts)
        {
            var recordsBuffer = new BlockingCollection<IList<Record>>();
            var searchTask = new Task(() => massiveFile.SearchRecords(recordsBuffer, new ColumnRecordSearch(query), 10000, maxResults, cts.Token), 
                cts.Token, TaskCreationOptions.LongRunning);
            searchTask.Start();

            await DisplayTaskResultsAsync(progress, cts.Token, recordsBuffer);
        }

        private static async Task DisplayTaskResultsAsync(IProgress<Record> progress, CancellationToken ct, BlockingCollection<IList<Record>> recordsBuffer)
        {
            var task = new Task(() =>
            {
                while (!recordsBuffer.IsCompleted)
                {
                    IList<Record> records = recordsBuffer.Take(ct);
                    foreach (var record in records)
                    {
                        progress.Report(record);
                    }
                }
            }, ct, TaskCreationOptions.LongRunning);

            task.Start();

            await task;
        }

        private void buttonSpeedTest_Click(object sender, EventArgs e)
        {
            double byteSumTime;
            var byteSum = GetByteSum(out byteSumTime);

            MessageBox.Show("Time to sum bytes: {0}s, Sum: {1}".FormatEx(byteSumTime, byteSum));

            double recordCountTime;
            var recordsCount = GetRecordsCount(out recordCountTime);
            MessageBox.Show("Time to count records: {0}s, Record Count: {1}".FormatEx(recordCountTime, recordsCount));
        }

        private long GetByteSum(out double byteSumTime)
        {
            var buffer = new byte[1 << 20];
            var sw = Stopwatch.StartNew();
            long byteSum = 0;
            using (var fs = File.OpenRead(this.textBoxFilePath.Text))
            {
                do
                {
                    var len = fs.Read(buffer, 0, buffer.Length);
                    for (var i = 0; i < len; i++)
                        byteSum += buffer[i];
                } while (fs.Position < fs.Length);
            }
            byteSumTime = sw.Elapsed.TotalSeconds;
            return byteSum;
        }

        private int GetRecordsCount(out double recordCountTime)
        {
            Stopwatch sw;
            var massive = new MassiveFile(this.textBoxFilePath.Text);
            var pageIndex = 0;
            var recordsCount = 0;
            sw = Stopwatch.StartNew();
            while (!massive.EndOfFile)
            {
                var allRecordsBuffer = new BlockingCollection<IList<Record>>();
                massive.GetRecords(pageIndex++, allRecordsBuffer, massive.PageSize, cts.Token, massive.PageSize);
                IList<Record> records;
                while (!allRecordsBuffer.IsCompleted && allRecordsBuffer.TryTake(out records))
                {
                    recordsCount += records.Count;
                }
            }
            recordCountTime = sw.Elapsed.TotalSeconds;
            return recordsCount;
        }

        private int GetSearchResultCount(out double searchResultCountTime)
        {
            Stopwatch sw;
            var massive = new MassiveFile(this.textBoxFilePath.Text);
            var pageIndex = 0;
            var recordsCount = 0;
            sw = Stopwatch.StartNew();
            while (!massive.EndOfFile)
            {
                var allRecordsBuffer = new BlockingCollection<IList<Record>>();
                massive.GetRecords(pageIndex++, allRecordsBuffer, massive.PageSize, cts.Token, massive.PageSize);
                IList<Record> records;
                while (!allRecordsBuffer.IsCompleted && allRecordsBuffer.TryTake(out records))
                {
                    recordsCount += records.Count;
                }
            }
            searchResultCountTime = sw.Elapsed.TotalSeconds;
            return recordsCount;
        }

    }
}
