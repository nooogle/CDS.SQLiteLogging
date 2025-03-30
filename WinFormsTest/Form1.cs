
using Microsoft.Extensions.Logging;

namespace WinFormsTest
{
    public partial class Form1 : Form
    {
        private CDS.SQLiteLogging.SQLiteLogger<MyLogEntry> logger;
        private int nextLineIndex = 1;


        public Form1()
        {
            InitializeComponent();
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var logFolder = LogFolderManager.GetLogFolder();

            logger = new CDS.SQLiteLogging.SQLiteLogger<MyLogEntry>(
                folder: logFolder,
                schemaVersion: MyLogEntry.Version,
                new CDS.SQLiteLogging.BatchingOptions(),
                new CDS.SQLiteLogging.HouseKeepingOptions());

            logger.DeleteAll();

            Text = Application.ProductName + " " + Application.ProductVersion + ", log folder [" + logFolder;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            var logEntry = new MyLogEntry()
            {
                Timestamp = DateTimeOffset.Now,
                Level = LogLevel.Information,
                LineIndex = nextLineIndex++,
                BatchNumber = "Test",
                Sender = nameof(Form1),
                MessageTemplate = "Timer tick",
            };

            logger.Add(logEntry);
        }
    }
}
