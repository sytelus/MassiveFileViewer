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

            if (massiveTextFile.Lines.Count > 0)
            {
                DataTable table = new DataTable();
                for (int i = 0; i < massiveTextFile.Lines[0].Split(Utils.TabDelimiter).Length; i++)
                    table.Columns.Add(i.ToString(), typeof(string));

                foreach (var line in massiveTextFile.Lines)
                    table.LoadDataRow(line.Split(Utils.TabDelimiter), true);

                dataGridViewMain.DataSource = table.DefaultView;
            }
            else
                this.dataGridViewMain.DataSource = null;
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
    }
}
