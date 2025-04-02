namespace WinFormsTest.DomainSpecificLiveLogViewer
{
    partial class FormDemo
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDemo));
            systemInfoPanel1 = new WinFormsTest.Utils.SystemInfoPanel();
            simpleLogViewList = new LiveLogViewList();
            btnClearLog = new Button();
            toolTip1 = new ToolTip(components);
            btnBake = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            panel1 = new Panel();
            propertyGrid = new PropertyGrid();
            tableLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // systemInfoPanel1
            // 
            systemInfoPanel1.Dock = DockStyle.Top;
            systemInfoPanel1.Location = new Point(0, 0);
            systemInfoPanel1.Name = "systemInfoPanel1";
            systemInfoPanel1.Size = new Size(685, 60);
            systemInfoPanel1.TabIndex = 1;
            // 
            // simpleLogViewList
            // 
            simpleLogViewList.Dock = DockStyle.Fill;
            simpleLogViewList.Location = new Point(370, 3);
            simpleLogViewList.MaxQueueSize = 1000;
            simpleLogViewList.Name = "simpleLogViewList";
            simpleLogViewList.Size = new Size(312, 362);
            simpleLogViewList.TabIndex = 2;
            // 
            // btnClearLog
            // 
            btnClearLog.Location = new Point(15, 15);
            btnClearLog.Name = "btnClearLog";
            btnClearLog.Size = new Size(75, 23);
            btnClearLog.TabIndex = 3;
            btnClearLog.Text = "Clear log";
            toolTip1.SetToolTip(btnClearLog, "Clears the displayed log entries and any entries cached for the UI");
            btnClearLog.UseVisualStyleBackColor = true;
            btnClearLog.Click += btnClearLog_Click;
            // 
            // btnBake
            // 
            btnBake.Location = new Point(96, 15);
            btnBake.Name = "btnBake";
            btnBake.Size = new Size(75, 23);
            btnBake.TabIndex = 5;
            btnBake.Text = "Bake";
            toolTip1.SetToolTip(btnBake, "Clears the displayed log entries and any entries cached for the UI");
            btnBake.UseVisualStyleBackColor = true;
            btnBake.Click += btnBake_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 367F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(simpleLogViewList, 1, 0);
            tableLayoutPanel1.Controls.Add(panel1, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 60);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(685, 368);
            tableLayoutPanel1.TabIndex = 4;
            // 
            // panel1
            // 
            panel1.Controls.Add(btnBake);
            panel1.Controls.Add(propertyGrid);
            panel1.Controls.Add(btnClearLog);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(3, 3);
            panel1.Name = "panel1";
            panel1.Padding = new Padding(12);
            panel1.Size = new Size(361, 362);
            panel1.TabIndex = 3;
            // 
            // propertyGrid
            // 
            propertyGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            propertyGrid.Location = new Point(15, 44);
            propertyGrid.Name = "propertyGrid";
            propertyGrid.PropertySort = PropertySort.NoSort;
            propertyGrid.Size = new Size(331, 303);
            propertyGrid.TabIndex = 4;
            // 
            // FormDemo
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(685, 428);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(systemInfoPanel1);
            Name = "FormDemo";
            Text = "Form1";
            Load += FormMain_Load;
            tableLayoutPanel1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Utils.SystemInfoPanel systemInfoPanel1;
        private LiveLogViewList simpleLogViewList;
        private Button btnClearLog;
        private ToolTip toolTip1;
        private TableLayoutPanel tableLayoutPanel1;
        private Panel panel1;
        private Button btnBake;
        private PropertyGrid propertyGrid;
    }
}
