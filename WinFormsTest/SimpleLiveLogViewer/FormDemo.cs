
using CDS.SQLiteLogging;
using CDS.SQLiteLogging.MEL;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WinFormsTest.SimpleListLogViewer
{
    public partial class FormDemo : Form
    {
        private ISQLiteWriterUtilities? loggerUtilities;
        private IServiceProvider? serviceProvider;
        private ILogger<FormDemo>? logger;

        //private int nextLineIndex = 1;


        public FormDemo()
        {
            InitializeComponent();
        }


        private string CreateDBPath() => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            nameof(CDS),
            nameof(CDS.SQLiteLogging),
            nameof(WinFormsTest),
            $"Log_V{MELLogger.DBSchemaVersion}.db");


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Text = Application.ProductName + " " + Application.ProductVersion;

            CreateLogger();
            logger?.LogInformation("FormMain loaded.");

            var eventID = new EventId(1, "Event1");
            logger?.LogCritical(eventID, "Testing a structure message for user {User}.", "Alice");
        }

        private void CreateLogger()
        {
            // Create the SQLite logger provider
            var sqliteLoggerProvider = MELLoggerProvider.Create(CreateDBPath());

            // Get the logger utilities - we want to make these available to the demo classes
            loggerUtilities = sqliteLoggerProvider.LoggerUtilities;

            // Setup dependency injection
            serviceProvider = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.AddProvider(sqliteLoggerProvider);
                    builder.SetMinimumLevel(LogLevel.Trace);
                })

                // Add SQLiteWriterUtilities
                .AddSingleton(loggerUtilities)

                // Build the service provider
                .BuildServiceProvider();

            // Add a handler for new log entries
            sqliteLoggerProvider.LogEntryReceived += simpleLogViewList.QueueLogEntry;

            // get a logger
            logger = serviceProvider.GetRequiredService<ILogger<FormDemo>>();
        }


        /// <summary>
        /// Handles the Tick event of the timer control.
        /// </summary>
        private void timer_Tick(object sender, EventArgs e)
        {
            logger?.LogTrace("Timer ticked.");
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            simpleLogViewList.Filter = simpleLogViewList_Filter;
        }

        private bool simpleLogViewList_Filter(LogEntry entry)
        {
            return entry.Level >= LogLevel.Information;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            simpleLogViewList.Clear();
        }
    }
}
