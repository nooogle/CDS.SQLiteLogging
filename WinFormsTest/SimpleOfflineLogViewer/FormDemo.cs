namespace WinFormsTest.SimpleOfflineLogViewer
{
    public partial class FormDemo : Form
    {
        private CDS.SQLiteLogging.Reader? reader;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormDemo"/> class.
        /// </summary>
        public FormDemo()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the click event of the File -> Exit menu item.
        /// Closes the form.
        /// </summary>
        private void menuFileExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Handles the click event of the File -> Load menu item.
        /// Opens a file dialog to select a database file and loads the log entries.
        /// </summary>
        private void menuFileLoad_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog(this) != DialogResult.OK) { return; }

            // Initialize the reader with the selected database file
            reader = new CDS.SQLiteLogging.Reader(openFileDialog.FileName);
            ReselectData();
        }

        /// <summary>
        /// Handles the KeyDown event of the textBoxQuery control.
        /// Detects if the Enter key is pressed and reselects data.
        /// </summary>
        private void textBoxQuery_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if the Enter key was pressed
            if (e.KeyCode == Keys.Enter)
            {
                ReselectData();
            }
        }

        /// <summary>
        /// Reselects data based on the query in the textBoxQuery control.
        /// Clears the current log view and loads new entries from the database.
        /// </summary>
        private void ReselectData()
        {
            if (reader == null) { return; }

            simpleLogView.Clear();
            try
            {
                // Execute the query and get log entries
                var entries = reader.Select(textBoxQuery.Text);

                // Add each log entry to the log view
                foreach (var entry in entries)
                {
                    simpleLogView.QueueLogEntry(entry);
                }
            }
            catch (Exception ex)
            {
                // Show an error message if an exception occurs
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the Load event of the form.
        /// Replaces the placeholder in the query text with the actual table name.
        /// </summary>
        private void FormDemo_Load(object sender, EventArgs e)
        {
            textBoxQuery.Text = textBoxQuery.Text.Replace("{tableName}", CDS.SQLiteLogging.Reader.TableName);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);


            // Dispose of the reader if it is not null
            reader?.Dispose();
            reader = null;
        }
    }
}
