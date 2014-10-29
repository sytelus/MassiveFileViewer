using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonUtils;
using CommonWinFormUtils;
using System.Threading.Tasks.Dataflow;
using System.Threading;

namespace MassiveFileViewer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        MassiveTextFile massiveTextFile = null;
        private void MainForm_Load(object sender, EventArgs e)
        {
            buttonLoadFile_Click(null, null);
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (massiveTextFile != null)
                massiveTextFile.Dispose();
        }

        private void buttonLoadFile_Click(object sender, EventArgs e)
        {
            if (massiveTextFile != null)
                massiveTextFile.Dispose();

            massiveTextFile = new MassiveTextFile();
            massiveTextFile.OnPageRefreshed += massiveTextFile_OnPageRefreshed;
            massiveTextFile.Load(textBoxFilePath.Text);
        }

        public class LineItem
        {
            public string Line { get; set; }
        }
        private void massiveTextFile_OnPageRefreshed(object sender, EventArgs args)
        {
            var approximatePrefix = massiveTextFile.IsPageApproximate(massiveTextFile.CurrentPageIndex) ? "~" : string.Empty;
            toolStripStatusLabelCurrentLine.Text = "Current Line: " + approximatePrefix  + massiveTextFile.CurrentLineEstimate;
            toolStripStatusLabelTotalLines.Text = string.Concat("Total Lines: ~", massiveTextFile.TotalLinesEstimate.ToString("N0"),
                "±", ((int)massiveTextFile.TotalLinesStandardDeviation).ToString("N0"));
            toolStripStatusLabelFileSize.Text = "File Size: " + massiveTextFile.FileSize;
            toolStripStatusLabelCurrentPosition.Text = @"Current Byte#: " + massiveTextFile.CurrentBytePosition;
            toolStripStatusLabelCurrentPage.Text = "Current Page:  " + approximatePrefix + massiveTextFile.CurrentPageIndex;
            toolStripStatusLabelTotalPages.Text = string.Concat("Total Pages: ~", massiveTextFile.TotalPagesEstimate.ToString("N0"),
                "±", ((int)massiveTextFile.TotalPagesStandardDeviation).ToString("N0"));
            textBoxCurrentPageIndex.Text = massiveTextFile.CurrentPageIndex.ToString();
            textBoxPageSize.Text = massiveTextFile.PageSize.ToStringInvariant();

            this.DisplayLines(massiveTextFile.Lines);
        }


        private void ClearGrid()
        {
            dataGridViewMain.ColumnCount = 0;
            dataGridViewMain.Rows.Clear();
        }

        private void AddLineInGrid(string[] line, long rowIndex)
        {
            if (dataGridViewMain.ColumnCount == 0)
            {
                dataGridViewMain.ColumnCount = line.Length;
                for (var columnIndex = 0; columnIndex < line.Length; columnIndex++)
                    dataGridViewMain.Columns[columnIndex].Name = columnIndex.ToStringCurrentCulture();
            }

            var index = dataGridViewMain.Rows.Add(line);
            dataGridViewMain.Rows[index].HeaderCell.Value = rowIndex.ToString();
        }

        private void DisplayLines(IEnumerable<string[]> lines)
        {
            this.ClearGrid();
            var lineCount = 0;
            foreach (var line in lines)
                this.AddLineInGrid(line, lineCount++);
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            massiveTextFile.CurrentPageIndex++;
        }

        private void buttonPrevious_Click(object sender, EventArgs e)
        {
            massiveTextFile.CurrentPageIndex--;
        }

        private void buttonGotoPage_Click(object sender, EventArgs e)
        {
            massiveTextFile.CurrentPageIndex = int.Parse(textBoxCurrentPageIndex.Text);
        }

        private void buttonChangePageSize_Click(object sender, EventArgs e)
        {
            massiveTextFile.PageSize = int.Parse(textBoxPageSize.Text);
        }

        //Task currentSearchTask = null;
        CancellationTokenSource cts = new CancellationTokenSource();
        long? pageIndexBeforeSearch = null;

        private async void buttonSearch_Click(object sender, EventArgs e)
        {
            this.pageIndexBeforeSearch = this.pageIndexBeforeSearch ?? massiveTextFile.CurrentPageIndex;
            this.progressBarSearch.Maximum = (int)massiveTextFile.TotalLinesEstimate;   //TODO: handle long balues properly
            this.progressBarSearch.Minimum = 0;
            this.ClearGrid();

            var progress = new Progress<MassiveTextFile.SearchResult>((record) =>
                {
                    if (!record.IsProgressReport)
                    {
                        this.AddLineInGrid(record.Columns, record.LineIndex);
                    }
                    this.progressBarSearch.Value = (int)record.LineIndex; //TODO: handle long balues properly
                    this.labelSearchProgress.Text = record.LineIndex.ToString();
                });

            await Task.Factory.StartNew(() => SearchAsync(textBoxQuery.Text, progress), TaskCreationOptions.LongRunning);
        }

        private async Task SearchAsync(string query, IProgress<MassiveTextFile.SearchResult> progress)
        {
            var filteredLinesBuffer = new BufferBlock<MassiveTextFile.SearchResult>();
            var task = massiveTextFile.SearchRecordsAsync(filteredLinesBuffer, new ColumnRecordSearch(query), cts.Token);

            var recordCount = 0;
            while (await filteredLinesBuffer.OutputAvailableAsync())
            {
                MassiveTextFile.SearchResult record;
                while (filteredLinesBuffer.TryReceive(out record))
                {
                    progress.Report(record);

                    if (!record.IsProgressReport)
                    {
                        recordCount++;

                        if (recordCount >= 50)
                            cts.Cancel();
                    }
                }
            }
        }
    }
}
