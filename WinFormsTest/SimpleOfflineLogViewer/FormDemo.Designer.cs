namespace WinFormsTest.SimpleOfflineLogViewer
{
    partial class FormDemo
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
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            menuFileLoad = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripSeparator();
            menuFileExit = new ToolStripMenuItem();
            openFileDialog = new OpenFileDialog();
            simpleLogView = new CDS.SQLiteLogging.Views.SimpleLogView();
            label1 = new Label();
            textBoxQuery = new TextBox();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(800, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { menuFileLoad, toolStripMenuItem1, menuFileExit });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // menuFileLoad
            // 
            menuFileLoad.Name = "menuFileLoad";
            menuFileLoad.Size = new Size(150, 22);
            menuFileLoad.Text = "Load database";
            menuFileLoad.Click += menuFileLoad_Click;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(147, 6);
            // 
            // menuFileExit
            // 
            menuFileExit.Name = "menuFileExit";
            menuFileExit.Size = new Size(150, 22);
            menuFileExit.Text = "Exit";
            menuFileExit.Click += menuFileExit_Click;
            // 
            // openFileDialog
            // 
            openFileDialog.Filter = "SQLite database files|*.db";
            // 
            // simpleLogView
            // 
            simpleLogView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            simpleLogView.Location = new Point(12, 74);
            simpleLogView.MaxQueueSize = 1000;
            simpleLogView.Name = "simpleLogView";
            simpleLogView.Size = new Size(776, 364);
            simpleLogView.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 34);
            label1.Name = "label1";
            label1.Size = new Size(42, 15);
            label1.TabIndex = 2;
            label1.Text = "Query:";
            // 
            // textBoxQuery
            // 
            textBoxQuery.AcceptsReturn = true;
            textBoxQuery.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxQuery.Location = new Point(60, 31);
            textBoxQuery.Name = "textBoxQuery";
            textBoxQuery.Size = new Size(728, 23);
            textBoxQuery.TabIndex = 3;
            textBoxQuery.Text = "SELECT * FROM {tableName} ORDER BY Timestamp DESC LIMIT 10;";
            textBoxQuery.KeyDown += textBoxQuery_KeyDown;
            // 
            // FormDemo
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(textBoxQuery);
            Controls.Add(label1);
            Controls.Add(simpleLogView);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "FormDemo";
            Text = "FormDemo";
            Load += FormDemo_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem menuFileLoad;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripMenuItem menuFileExit;
        private OpenFileDialog openFileDialog;
        private CDS.SQLiteLogging.Views.SimpleLogView simpleLogView;
        private Label label1;
        private TextBox textBoxQuery;
    }
}