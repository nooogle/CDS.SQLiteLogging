
using CDS.SQLiteLogging;
using CDS.SQLiteLogging.MEL;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WinFormsTest.DomainSpecificLiveLogViewer
{
    /// <summary>
    /// Represents the main form for the application.
    /// </summary>
    public partial class FormDemo : Form
    {
        private ISQLiteWriterUtilities? loggerUtilities;
        private IServiceProvider? serviceProvider;
        private ILogger<FormDemo>? logger;
        private readonly BreadOrder[] breadOrders;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormDemo"/> class.
        /// </summary>
        public FormDemo()
        {
            InitializeComponent();
            breadOrders = CreateDefaultBreadOrders();
        }

        /// <summary>
        /// Creates the default bread orders.
        /// </summary>
        /// <returns>An array of default bread orders.</returns>
        private static BreadOrder[] CreateDefaultBreadOrders()
        {
            return
            [
                new BreadOrder
                {
                    BatchNumber = "Mr Miller 01",
                    FlourType = "Whole Wheat",
                    NumberOfLoafs = 5
                },
                new BreadOrder
                {
                    BatchNumber = "Mr Miller 02",
                    FlourType = "White",
                    NumberOfLoafs = 3
                },
                new BreadOrder
                {
                    BatchNumber = "Mr Miller 03",
                    FlourType = "Rye",
                    NumberOfLoafs = 2
                }
            ];
        }

        /// <summary>
        /// Creates the database path for the SQLite logger.
        /// </summary>
        /// <returns>The database path.</returns>
        private string CreateDBPath() => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            nameof(CDS),
            nameof(CDS.SQLiteLogging),
            nameof(WinFormsTest),
            $"Log_V{MELLogger.DBSchemaVersion}.db");

        /// <summary>
        /// Handles the load event of the form.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Text = Application.ProductName + " " + Application.ProductVersion;
            CreateLogger();
        }

        /// <summary>
        /// Creates and configures the logger.
        /// </summary>
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
                .AddSingleton(loggerUtilities)
                .AddTransient<BreadFactory>()
                .BuildServiceProvider();

            // Add a handler for new log entries
            sqliteLoggerProvider.LogEntryReceived += simpleLogViewList.QueueLogEntry;

            // Get a logger
            logger = serviceProvider.GetRequiredService<ILogger<FormDemo>>();
            logger.LogDebug("All services created, ready to make some bread!");
        }

        /// <summary>
        /// Handles the load event of the main form.
        /// </summary>
        private void FormMain_Load(object sender, EventArgs e)
        {
            simpleLogViewList.Filter = simpleLogViewList_Filter;
            propertyGrid.SelectedObject = breadOrders;
            propertyGrid.ExpandAllGridItems();
        }

        /// <summary>
        /// Filters log entries to display only those with a level of Information or higher.
        /// </summary>
        /// <param name="entry">The log entry to filter.</param>
        /// <returns>True if the log entry should be displayed; otherwise, false.</returns>
        private bool simpleLogViewList_Filter(LogEntry entry)
        {
            return entry.Level >= LogLevel.Debug;
        }

        /// <summary>
        /// Handles the click event of the clear button.
        /// </summary>
        private void btnClearLog_Click(object sender, EventArgs e)
        {
            simpleLogViewList.Clear();
        }

        /// <summary>
        /// Handles the click event of the bake button to start baking bread.
        /// </summary>
        private async void btnBake_Click(object sender, EventArgs e)
        {
            if (serviceProvider == null)
            {
                logger?.LogError("Service provider is not initialized.");
                return;
            }

            logger?.LogDebug("User clicked the bake button.");

            btnBake.Enabled = false;
            propertyGrid.Enabled = false;

            var breadFactory = serviceProvider.GetRequiredService<BreadFactory>();
            await breadFactory.MakeBread(breadOrders);

            propertyGrid.Enabled = true;
            btnBake.Enabled = true;

            logger?.LogDebug("The simulated bread baking is complete!");
        }
    }
}
