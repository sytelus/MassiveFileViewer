using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using CommonUtils;
using CommonWinFormUtils;
using System.Threading.Tasks.Dataflow;
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
            var recordsBuffer = new BufferBlock<Record>();
            var recordsTask = massiveFile.GetRecordsAsync(pageIndex, recordsBuffer, ct);
            await DisplayTaskResults(progress, ct, recordsBuffer, recordsTask);
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
            long recordCount = 0;
            var sw = Stopwatch.StartNew();

            var progress = new Progress<Record>((record) =>
            {
                if (!record.IsProgressReport)
                {
                    this.AddRecordInGrid(record, record.RecordIndex);
                }
                this.progressBarSearch.Value = (int)++recordCount;
                this.labelSearchProgress.Text = record.RecordIndex.ToString("N0");

                var throughput = sw.Elapsed.TotalMilliseconds/recordCount;  //millisecond/record
                var eta = throughput*maxRecordsExpected;
                this.labelEta.Text = @"{0} ms/record, ETA: {1}".FormatEx(throughput.ToString("N0"),
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
            var recordsBuffer = new BufferBlock<Record>();
            var searchTask = massiveFile.SearchRecordsAsync(recordsBuffer, new ColumnRecordSearch(query), maxResults, cts.Token);

            await DisplayTaskResults(progress, cts.Token, recordsBuffer, searchTask);
        }

        private static async Task DisplayTaskResults(IProgress<Record> progress, CancellationToken ct, IReceivableSourceBlock<Record> recordsBuffer, Task task)
        {
            while (await recordsBuffer.OutputAvailableAsync(ct))
            {
                Record record;
                while (recordsBuffer.TryReceive(out record))
                {
                    progress.Report(record);
                }
            }

            await task;
        }
    }
}
