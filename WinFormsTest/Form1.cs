
namespace WinFormsTest
{
    public partial class Form1 : Form
    {
        private CDS.SQLiteLogging.Logger<MyLogEntry> logger;
        private int nextLineIndex = 1;


        public Form1()
        {
            InitializeComponent();
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var logFolder = LogFolderManager.GetLogFolder();

            logger = new CDS.SQLiteLogging.Logger<MyLogEntry>(
                folder: logFolder,
                schemaVersion: MyLogEntry.Version,
                new CDS.SQLiteLogging.BatchingOptions(),
                new CDS.SQLiteLogging.HouseKeepingOptions());

            logger.DeleteAll();

            Text = $"Log folder: {logFolder}";
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            var logEntry = new MyLogEntry()
            {
                Timestamp = DateTimeOffset.Now,
                Level = CDS.SQLiteLogging.LogLevel.Information,
                LineIndex = nextLineIndex++,
                BatchNumber = "Test",
                Sender = nameof(Form1),
                Message = "Timer tick",
            };

            logger.Add(logEntry);
        }
    }
}
