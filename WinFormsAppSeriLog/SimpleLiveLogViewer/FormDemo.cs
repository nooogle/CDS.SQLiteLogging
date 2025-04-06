using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Observable;
using System;
using System.Reactive.Linq;


namespace WinFormsAppSeriLog.SimpleListLogViewer
{
    public partial class FormDemo : Form
    {
        private Serilog.Core.Logger? logger;


        public FormDemo()
        {
            InitializeComponent();
        }


        private string CreateDBPath() => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            nameof(WinFormsAppSeriLog),
            $"Log.db");


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Text = Application.ProductName + " " + Application.ProductVersion;

            CreateLogger();
            logger?.Information("FormMain loaded.");

            Task.Run(() =>
            {                 // Simulate some work
                for (int i = 0; i < 4; i++)
                {
                    Thread.Sleep(100);
                    logger?.Information("Simulated work {Index}", i);
                }
            });
        }

        private void CreateLogger()
        {
            // Configure Serilog with both SQLite and Observable sinks
            logger = new LoggerConfiguration()
                .WriteTo.SQLite(CreateDBPath(), storeTimestampInUtc: true, tableName: "LogEntries")

                .WriteTo.Observers(events => events
                    .Do(evt => {
                        simpleLogViewList.QueueLogEntry2(evt);
                    })
                    .Subscribe())

                .Enrich.WithExceptionDetails()

                .CreateLogger();

            // Generate sample logs
            CreateSampleLogEntries(logger);
        }

        private static void CreateSampleLogEntries(Serilog.Core.Logger logger)
        {
            // log all the different levels of messages
            logger.Verbose("This is a verbose message");
            logger.Debug("This is a debug message");
            logger.Information("This is an information message");
            logger.Warning("This is a warning message");
            logger.Error("This is an error message");
            logger.Fatal("This is a fatal message");
            logger.Information("This is an information message with a parameter: {Parameter}", "TestParameter");
            logger.Information("This is an information message with a structure: {@Structure}", new { Name = "Test", Value = 123 });
            logger.Information("This is an information message with a list: {@List}", new List<string> { "Item1", "Item2", "Item3" });
            logger.Information("This is an information message with a dictionary: {@Dictionary}", new Dictionary<string, string> { { "Key1", "Value1" }, { "Key2", "Value2" } });

            // Log an exception
            try
            {
                CreateException();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Caught an exception!");
            }
        }

        /// <summary>
        /// Creates an exception with an inner exception.
        /// </summary>
        private static Exception? CreateException()
        {
            try
            {
                CreateInnerException();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("This is a demo exception!", ex);
            }

            return null;
        }

        /// <summary>
        /// Creates an inner exception.
        /// </summary>
        private static void CreateInnerException()
        {
            throw new NotImplementedException("This is another demo exception");
        }


        /// <summary>
        /// Handles the Tick event of the timer control.
        /// </summary>
        private void timer_Tick(object sender, EventArgs e)
        {
            //logger?.LogTrace("Timer ticked.");
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            //simpleLogViewList.Filter = simpleLogViewList_Filter;
        }

        //private bool simpleLogViewList_Filter(LogEntry entry)
        //{
        //    return entry.Level >= LogLevel.Information;
        //}

        private void btnClear_Click(object sender, EventArgs e)
        {
            simpleLogViewList.Clear();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            Log.CloseAndFlush();
        }
    }
}
