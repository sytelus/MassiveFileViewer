namespace MassiveFileViewer
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.dataGridViewMain = new System.Windows.Forms.DataGridView();
            this.panel4 = new System.Windows.Forms.Panel();
            this.buttonChangePageSize = new System.Windows.Forms.Button();
            this.textBoxPageSize = new System.Windows.Forms.TextBox();
            this.buttonGotoPage = new System.Windows.Forms.Button();
            this.textBoxCurrentPageIndex = new System.Windows.Forms.TextBox();
            this.buttonNext = new System.Windows.Forms.Button();
            this.buttonPrevious = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.labelEta = new System.Windows.Forms.Label();
            this.labelSearchProgress = new System.Windows.Forms.Label();
            this.progressBarSearch = new System.Windows.Forms.ProgressBar();
            this.buttonSearch = new System.Windows.Forms.Button();
            this.textBoxQuery = new System.Windows.Forms.TextBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.buttonLoadFile = new System.Windows.Forms.Button();
            this.textBoxFilePath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabelCurrentPage = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelTotalPages = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelCurrentRecord = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelTotalRecords = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelCurrentPosition = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelFileSize = new System.Windows.Forms.ToolStripStatusLabel();
            this.buttonSpeedTest = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMain)).BeginInit();
            this.panel4.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.dataGridViewMain);
            this.panel1.Controls.Add(this.panel4);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 37);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(830, 628);
            this.panel1.TabIndex = 0;
            // 
            // dataGridViewMain
            // 
            this.dataGridViewMain.AllowUserToAddRows = false;
            this.dataGridViewMain.AllowUserToDeleteRows = false;
            this.dataGridViewMain.AllowUserToOrderColumns = true;
            this.dataGridViewMain.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.dataGridViewMain.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewMain.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridViewMain.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewMain.Name = "dataGridViewMain";
            this.dataGridViewMain.ReadOnly = true;
            this.dataGridViewMain.Size = new System.Drawing.Size(830, 575);
            this.dataGridViewMain.TabIndex = 0;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.buttonChangePageSize);
            this.panel4.Controls.Add(this.textBoxPageSize);
            this.panel4.Controls.Add(this.buttonGotoPage);
            this.panel4.Controls.Add(this.textBoxCurrentPageIndex);
            this.panel4.Controls.Add(this.buttonNext);
            this.panel4.Controls.Add(this.buttonPrevious);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel4.Location = new System.Drawing.Point(0, 575);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(830, 53);
            this.panel4.TabIndex = 1;
            // 
            // buttonChangePageSize
            // 
            this.buttonChangePageSize.Location = new System.Drawing.Point(87, 4);
            this.buttonChangePageSize.Name = "buttonChangePageSize";
            this.buttonChangePageSize.Size = new System.Drawing.Size(120, 23);
            this.buttonChangePageSize.TabIndex = 5;
            this.buttonChangePageSize.Text = "Change Page Size";
            this.buttonChangePageSize.UseVisualStyleBackColor = true;
            this.buttonChangePageSize.Click += new System.EventHandler(this.buttonChangePageSize_Click);
            // 
            // textBoxPageSize
            // 
            this.textBoxPageSize.Location = new System.Drawing.Point(4, 6);
            this.textBoxPageSize.Name = "textBoxPageSize";
            this.textBoxPageSize.Size = new System.Drawing.Size(77, 20);
            this.textBoxPageSize.TabIndex = 4;
            // 
            // buttonGotoPage
            // 
            this.buttonGotoPage.Location = new System.Drawing.Point(541, 4);
            this.buttonGotoPage.Name = "buttonGotoPage";
            this.buttonGotoPage.Size = new System.Drawing.Size(75, 23);
            this.buttonGotoPage.TabIndex = 3;
            this.buttonGotoPage.Text = "Goto Page";
            this.buttonGotoPage.UseVisualStyleBackColor = true;
            this.buttonGotoPage.Click += new System.EventHandler(this.buttonGotoPage_Click);
            // 
            // textBoxCurrentPageIndex
            // 
            this.textBoxCurrentPageIndex.Location = new System.Drawing.Point(458, 6);
            this.textBoxCurrentPageIndex.Name = "textBoxCurrentPageIndex";
            this.textBoxCurrentPageIndex.Size = new System.Drawing.Size(77, 20);
            this.textBoxCurrentPageIndex.TabIndex = 2;
            // 
            // buttonNext
            // 
            this.buttonNext.Location = new System.Drawing.Point(749, 3);
            this.buttonNext.Name = "buttonNext";
            this.buttonNext.Size = new System.Drawing.Size(75, 23);
            this.buttonNext.TabIndex = 1;
            this.buttonNext.Text = "Next >>";
            this.buttonNext.UseVisualStyleBackColor = true;
            this.buttonNext.Click += new System.EventHandler(this.buttonNext_Click);
            // 
            // buttonPrevious
            // 
            this.buttonPrevious.Location = new System.Drawing.Point(668, 3);
            this.buttonPrevious.Name = "buttonPrevious";
            this.buttonPrevious.Size = new System.Drawing.Size(75, 23);
            this.buttonPrevious.TabIndex = 0;
            this.buttonPrevious.Text = "<< Previous";
            this.buttonPrevious.UseVisualStyleBackColor = true;
            this.buttonPrevious.Click += new System.EventHandler(this.buttonPrevious_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.buttonSpeedTest);
            this.panel2.Controls.Add(this.labelEta);
            this.panel2.Controls.Add(this.labelSearchProgress);
            this.panel2.Controls.Add(this.progressBarSearch);
            this.panel2.Controls.Add(this.buttonSearch);
            this.panel2.Controls.Add(this.textBoxQuery);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(830, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(200, 687);
            this.panel2.TabIndex = 1;
            // 
            // labelEta
            // 
            this.labelEta.AutoSize = true;
            this.labelEta.Location = new System.Drawing.Point(13, 148);
            this.labelEta.Name = "labelEta";
            this.labelEta.Size = new System.Drawing.Size(28, 13);
            this.labelEta.TabIndex = 4;
            this.labelEta.Text = "ETA";
            // 
            // labelSearchProgress
            // 
            this.labelSearchProgress.AutoSize = true;
            this.labelSearchProgress.Location = new System.Drawing.Point(149, 117);
            this.labelSearchProgress.Name = "labelSearchProgress";
            this.labelSearchProgress.Size = new System.Drawing.Size(0, 13);
            this.labelSearchProgress.TabIndex = 3;
            this.labelSearchProgress.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // progressBarSearch
            // 
            this.progressBarSearch.Location = new System.Drawing.Point(16, 111);
            this.progressBarSearch.Name = "progressBarSearch";
            this.progressBarSearch.Size = new System.Drawing.Size(167, 15);
            this.progressBarSearch.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBarSearch.TabIndex = 2;
            // 
            // buttonSearch
            // 
            this.buttonSearch.Location = new System.Drawing.Point(109, 65);
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.Size = new System.Drawing.Size(75, 23);
            this.buttonSearch.TabIndex = 1;
            this.buttonSearch.Text = "Search";
            this.buttonSearch.UseVisualStyleBackColor = true;
            this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);
            // 
            // textBoxQuery
            // 
            this.textBoxQuery.Location = new System.Drawing.Point(14, 39);
            this.textBoxQuery.Name = "textBoxQuery";
            this.textBoxQuery.Size = new System.Drawing.Size(170, 20);
            this.textBoxQuery.TabIndex = 0;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.buttonLoadFile);
            this.panel3.Controls.Add(this.textBoxFilePath);
            this.panel3.Controls.Add(this.label1);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(830, 37);
            this.panel3.TabIndex = 2;
            // 
            // buttonLoadFile
            // 
            this.buttonLoadFile.Location = new System.Drawing.Point(594, 4);
            this.buttonLoadFile.Name = "buttonLoadFile";
            this.buttonLoadFile.Size = new System.Drawing.Size(75, 23);
            this.buttonLoadFile.TabIndex = 2;
            this.buttonLoadFile.Text = "Load";
            this.buttonLoadFile.UseVisualStyleBackColor = true;
            this.buttonLoadFile.Click += new System.EventHandler(this.buttonLoadFile_Click);
            // 
            // textBoxFilePath
            // 
            this.textBoxFilePath.Location = new System.Drawing.Point(32, 6);
            this.textBoxFilePath.Name = "textBoxFilePath";
            this.textBoxFilePath.Size = new System.Drawing.Size(556, 20);
            this.textBoxFilePath.TabIndex = 1;
            this.textBoxFilePath.Text = "C:\\Temp\\url_file.tsv";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(26, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "File:";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelCurrentPage,
            this.toolStripStatusLabelTotalPages,
            this.toolStripStatusLabelCurrentRecord,
            this.toolStripStatusLabelTotalRecords,
            this.toolStripStatusLabelCurrentPosition,
            this.toolStripStatusLabelFileSize});
            this.statusStrip1.Location = new System.Drawing.Point(0, 665);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(830, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabelCurrentPage
            // 
            this.toolStripStatusLabelCurrentPage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabelCurrentPage.Name = "toolStripStatusLabelCurrentPage";
            this.toolStripStatusLabelCurrentPage.Size = new System.Drawing.Size(73, 17);
            this.toolStripStatusLabelCurrentPage.Text = "CurrentPage";
            // 
            // toolStripStatusLabelTotalPages
            // 
            this.toolStripStatusLabelTotalPages.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabelTotalPages.Name = "toolStripStatusLabelTotalPages";
            this.toolStripStatusLabelTotalPages.Size = new System.Drawing.Size(65, 17);
            this.toolStripStatusLabelTotalPages.Text = "TotalPages";
            // 
            // toolStripStatusLabelCurrentRecord
            // 
            this.toolStripStatusLabelCurrentRecord.Name = "toolStripStatusLabelCurrentRecord";
            this.toolStripStatusLabelCurrentRecord.Size = new System.Drawing.Size(84, 17);
            this.toolStripStatusLabelCurrentRecord.Text = "CurrentRecord";
            // 
            // toolStripStatusLabelTotalRecords
            // 
            this.toolStripStatusLabelTotalRecords.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabelTotalRecords.Name = "toolStripStatusLabelTotalRecords";
            this.toolStripStatusLabelTotalRecords.Size = new System.Drawing.Size(76, 17);
            this.toolStripStatusLabelTotalRecords.Text = "TotalRecords";
            // 
            // toolStripStatusLabelCurrentPosition
            // 
            this.toolStripStatusLabelCurrentPosition.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabelCurrentPosition.Name = "toolStripStatusLabelCurrentPosition";
            this.toolStripStatusLabelCurrentPosition.Size = new System.Drawing.Size(90, 17);
            this.toolStripStatusLabelCurrentPosition.Text = "CurrentPosition";
            // 
            // toolStripStatusLabelFileSize
            // 
            this.toolStripStatusLabelFileSize.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabelFileSize.Name = "toolStripStatusLabelFileSize";
            this.toolStripStatusLabelFileSize.Size = new System.Drawing.Size(45, 17);
            this.toolStripStatusLabelFileSize.Text = "FileSize";
            // 
            // buttonSpeedTest
            // 
            this.buttonSpeedTest.Location = new System.Drawing.Point(96, 650);
            this.buttonSpeedTest.Name = "buttonSpeedTest";
            this.buttonSpeedTest.Size = new System.Drawing.Size(92, 25);
            this.buttonSpeedTest.TabIndex = 5;
            this.buttonSpeedTest.Text = "Speed Test";
            this.buttonSpeedTest.UseVisualStyleBackColor = true;
            this.buttonSpeedTest.Click += new System.EventHandler(this.buttonSpeedTest_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1030, 687);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.panel2);
            this.Name = "MainForm";
            this.Text = "Massive File Viewer";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMain)).EndInit();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.DataGridView dataGridViewMain;
        private System.Windows.Forms.Button buttonLoadFile;
        private System.Windows.Forms.TextBox textBoxFilePath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonNext;
        private System.Windows.Forms.Button buttonPrevious;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelCurrentPage;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelTotalPages;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelCurrentRecord;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelTotalRecords;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelCurrentPosition;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelFileSize;
        private System.Windows.Forms.TextBox textBoxCurrentPageIndex;
        private System.Windows.Forms.Button buttonGotoPage;
        private System.Windows.Forms.Button buttonChangePageSize;
        private System.Windows.Forms.TextBox textBoxPageSize;
        private System.Windows.Forms.Button buttonSearch;
        private System.Windows.Forms.TextBox textBoxQuery;
        private System.Windows.Forms.ProgressBar progressBarSearch;
        private System.Windows.Forms.Label labelSearchProgress;
        private System.Windows.Forms.Label labelEta;
        private System.Windows.Forms.Button buttonSpeedTest;
    }
}

