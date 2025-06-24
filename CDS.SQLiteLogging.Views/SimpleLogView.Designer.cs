namespace CDS.SQLiteLogging.Views
{
    partial class SimpleLogView
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
            listViewLogEntries = new DoubleBufferedListView();
            columnHeaderID = new ColumnHeader();
            columnHeaderTime = new ColumnHeader();
            columnHeaderCategory = new ColumnHeader();
            columnHeaderMsg = new ColumnHeader();
            columnHeaderScopes = new ColumnHeader();
            columnHeaderException = new ColumnHeader();
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
            listViewLogEntries.Columns.AddRange(new ColumnHeader[] { columnHeaderID, columnHeaderTime, columnHeaderCategory, columnHeaderMsg, columnHeaderScopes, columnHeaderException });
            listViewLogEntries.Dock = DockStyle.Fill;
            listViewLogEntries.FullRowSelect = true;
            listViewLogEntries.GridLines = true;
            listViewLogEntries.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            listViewLogEntries.Location = new Point(0, 0);
            listViewLogEntries.Name = "listViewLogEntries";
            listViewLogEntries.Size = new Size(754, 204);
            listViewLogEntries.TabIndex = 0;
            listViewLogEntries.UseCompatibleStateImageBehavior = false;
            listViewLogEntries.View = View.Details;
            // 
            // columnHeaderID
            // 
            columnHeaderID.Text = "ID";
            // 
            // columnHeaderTime
            // 
            columnHeaderTime.Text = "Time";
            columnHeaderTime.Width = 120;
            // 
            // columnHeaderCategory
            // 
            columnHeaderCategory.Text = "Category";
            columnHeaderCategory.Width = 120;
            // 
            // columnHeaderMsg
            // 
            columnHeaderMsg.Text = "Message";
            columnHeaderMsg.Width = 400;
            // 
            // columnHeaderScopes
            // 
            columnHeaderScopes.Text = "Scope(s)";
            columnHeaderScopes.Width = 120;
            // 
            // columnHeaderException
            // 
            columnHeaderException.Text = "Exception";
            columnHeaderException.Width = 120;
            // 
            // SimpleLogView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(listViewLogEntries);
            Name = "SimpleLogView";
            Size = new Size(754, 204);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Timer timerGrabPendingLogEntries;
        private DoubleBufferedListView listViewLogEntries;
        private ColumnHeader columnHeaderTime;
        private ColumnHeader columnHeaderMsg;
        private ColumnHeader columnHeaderCategory;
        private ColumnHeader columnHeaderID;
        private ColumnHeader columnHeaderScopes;
        private ColumnHeader columnHeaderException;
    }
}
