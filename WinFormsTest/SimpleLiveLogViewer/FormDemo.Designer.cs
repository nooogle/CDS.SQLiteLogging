namespace WinFormsTest.SimpleListLogViewer
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
            timer = new System.Windows.Forms.Timer(components);
            systemInfoPanel1 = new WinFormsTest.Utils.SystemInfoPanel();
            simpleLogViewList = new SimpleLogViewList();
            btnClear = new Button();
            toolTip1 = new ToolTip(components);
            SuspendLayout();
            // 
            // timer
            // 
            timer.Enabled = true;
            timer.Interval = 5000;
            timer.Tick += timer_Tick;
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
            simpleLogViewList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            simpleLogViewList.Location = new Point(12, 95);
            simpleLogViewList.MaxQueueSize = 1000;
            simpleLogViewList.Name = "simpleLogViewList";
            simpleLogViewList.Size = new Size(661, 321);
            simpleLogViewList.TabIndex = 2;
            // 
            // btnClear
            // 
            btnClear.Location = new Point(12, 66);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(75, 23);
            btnClear.TabIndex = 3;
            btnClear.Text = "Clear";
            toolTip1.SetToolTip(btnClear, "Clears the displayed log entries and any entries cached for the UI");
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(685, 428);
            Controls.Add(btnClear);
            Controls.Add(simpleLogViewList);
            Controls.Add(systemInfoPanel1);
            Name = "FormMain";
            Text = "Form1";
            Load += FormMain_Load;
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Timer timer;
        private Utils.SystemInfoPanel systemInfoPanel1;
        private SimpleLogViewList simpleLogViewList;
        private Button btnClear;
        private ToolTip toolTip1;
    }
}
