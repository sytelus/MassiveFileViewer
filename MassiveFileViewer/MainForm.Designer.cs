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
            this.panel4 = new System.Windows.Forms.Panel();
            this.dataGridViewMain = new System.Windows.Forms.DataGridView();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.buttonLoadFile = new System.Windows.Forms.Button();
            this.textBoxFilePath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.buttonPrevious = new System.Windows.Forms.Button();
            this.buttonNext = new System.Windows.Forms.Button();
            this.toolStripStatusLabelCurrentPage = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelTotalPages = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelCurrentLine = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelTotalLines = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelCurrentPosition = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelFileSize = new System.Windows.Forms.ToolStripStatusLabel();
            this.textBoxCurrentPageIndex = new System.Windows.Forms.TextBox();
            this.buttonGotoPage = new System.Windows.Forms.Button();
            this.buttonChangePageSize = new System.Windows.Forms.Button();
            this.textBoxPageSize = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMain)).BeginInit();
            this.panel3.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panel4);
            this.panel1.Controls.Add(this.dataGridViewMain);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 37);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(830, 628);
            this.panel1.TabIndex = 0;
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
            this.dataGridViewMain.Size = new System.Drawing.Size(830, 628);
            this.dataGridViewMain.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(830, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(200, 687);
            this.panel2.TabIndex = 1;
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
            this.toolStripStatusLabelCurrentLine,
            this.toolStripStatusLabelTotalLines,
            this.toolStripStatusLabelCurrentPosition,
            this.toolStripStatusLabelFileSize});
            this.statusStrip1.Location = new System.Drawing.Point(0, 665);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(830, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
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
            // toolStripStatusLabelCurrentLine
            // 
            this.toolStripStatusLabelCurrentLine.Name = "toolStripStatusLabelCurrentLine";
            this.toolStripStatusLabelCurrentLine.Size = new System.Drawing.Size(69, 17);
            this.toolStripStatusLabelCurrentLine.Text = "CurrentLine";
            // 
            // toolStripStatusLabelTotalLines
            // 
            this.toolStripStatusLabelTotalLines.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabelTotalLines.Name = "toolStripStatusLabelTotalLines";
            this.toolStripStatusLabelTotalLines.Size = new System.Drawing.Size(61, 17);
            this.toolStripStatusLabelTotalLines.Text = "TotalLines";
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
            // textBoxCurrentPageIndex
            // 
            this.textBoxCurrentPageIndex.Location = new System.Drawing.Point(458, 6);
            this.textBoxCurrentPageIndex.Name = "textBoxCurrentPageIndex";
            this.textBoxCurrentPageIndex.Size = new System.Drawing.Size(77, 20);
            this.textBoxCurrentPageIndex.TabIndex = 2;
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
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMain)).EndInit();
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
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelCurrentLine;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelTotalLines;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelCurrentPosition;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelFileSize;
        private System.Windows.Forms.TextBox textBoxCurrentPageIndex;
        private System.Windows.Forms.Button buttonGotoPage;
        private System.Windows.Forms.Button buttonChangePageSize;
        private System.Windows.Forms.TextBox textBoxPageSize;
    }
}

