namespace WinFormsAppSeriLog.SimpleListLogViewer
{
    partial class SimpleLogViewList
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            timerGrabPendingLogEntries = new System.Windows.Forms.Timer(components);
            listViewLogEntries = new ListView();
            columnHeaderLiveID = new ColumnHeader();
            columnHeaderType = new ColumnHeader();
            columnHeaderTime = new ColumnHeader();
            columnHeaderMsg = new ColumnHeader();
            SuspendLayout();
            // 
            // timerGrabPendingLogEntries
            // 
            timerGrabPendingLogEntries.Enabled = true;
            timerGrabPendingLogEntries.Interval = 500;
            timerGrabPendingLogEntries.Tick += timerGrabPendingLogEntries_Tick;
            // 
            // listViewLogEntries
            // 
            listViewLogEntries.Columns.AddRange(new ColumnHeader[] { columnHeaderLiveID, columnHeaderType, columnHeaderTime, columnHeaderMsg });
            listViewLogEntries.Dock = DockStyle.Fill;
            listViewLogEntries.FullRowSelect = true;
            listViewLogEntries.GridLines = true;
            listViewLogEntries.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            listViewLogEntries.Location = new Point(0, 0);
            listViewLogEntries.MultiSelect = false;
            listViewLogEntries.Name = "listViewLogEntries";
            listViewLogEntries.Size = new Size(781, 204);
            listViewLogEntries.TabIndex = 0;
            listViewLogEntries.UseCompatibleStateImageBehavior = false;
            listViewLogEntries.View = View.Details;
            // 
            // columnHeaderLiveID
            // 
            columnHeaderLiveID.Text = "LiveId";
            // 
            // columnHeaderType
            // 
            columnHeaderType.Text = "Level";
            columnHeaderType.Width = 100;
            // 
            // columnHeaderTime
            // 
            columnHeaderTime.Text = "Time";
            columnHeaderTime.Width = 140;
            // 
            // columnHeaderMsg
            // 
            columnHeaderMsg.Text = "Message";
            columnHeaderMsg.Width = 400;
            // 
            // SimpleLogViewList
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(listViewLogEntries);
            Name = "SimpleLogViewList";
            Size = new Size(781, 204);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Timer timerGrabPendingLogEntries;
        private ListView listViewLogEntries;
        private ColumnHeader columnHeaderTime;
        private ColumnHeader columnHeaderMsg;
        private ColumnHeader columnHeaderType;
        private ColumnHeader columnHeaderLiveID;
    }
}
