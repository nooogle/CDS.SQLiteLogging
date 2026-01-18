using CDS.SQLiteLogging;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace WinFormsTest.DomainSpecificLiveLogViewer;

/// <summary>
/// A user control that displays live log entries.
/// </summary>
public partial class LiveLogViewList : UserControl
{
    private readonly LogEntryUICache logEntryUICache;

    /// <summary>
    /// Gets or sets the maximum queue size for log entries.
    /// </summary>
    /// <remarks>
    /// This property is not intended to be serialized by designer or code generation tools.
    /// </remarks>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int MaxQueueSize
    {
        get => logEntryUICache.MaxQueueSize;
        set => logEntryUICache.MaxQueueSize = value;
    }

    /// <summary>
    /// Gets or sets the filter function for log entries.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Func<LogEntry, bool>? Filter
    {
        get => logEntryUICache.Filter;
        set => logEntryUICache.Filter = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LiveLogViewList"/> class.
    /// </summary>
    public LiveLogViewList()
    {
        InitializeComponent();
        logEntryUICache = new LogEntryUICache();
    }

    /// <summary>
    /// Queues a log entry to be displayed.
    /// </summary>
    /// <param name="entry">The log entry to queue.</param>
    public void QueueLogEntry(LogEntry entry)
    {
        logEntryUICache.Add(entry);
    }

    /// <summary>
    /// Handles the Tick event of the timer control to grab pending log entries.
    /// </summary>
    private void timerGrabPendingLogEntries_Tick(object sender, EventArgs e)
    {
        DequeueAndDisplayCachedLogEntries();
    }

    /// <summary>
    /// Dequeues and displays cached log entries.
    /// </summary>
    private void DequeueAndDisplayCachedLogEntries()
    {
        var cachedLogEntries = logEntryUICache.DequeueAll();
        if (cachedLogEntries == null)
        {
            return;
        }

        foreach (var entry in cachedLogEntries)
        {
            InsertLogEntry(entry);
        }
    }

    /// <summary>
    /// Inserts a log entry into the ListView and auto-resizes the columns to fit the content.
    /// </summary>
    /// <param name="entry">The log entry to insert.</param>
    private void InsertLogEntry(LogEntry entry)
    {
        var localTime = entry.Timestamp.ToLocalTime();

        var listViewItem = new ListViewItem(localTime.ToString("T"))
        {
            Tag = entry
        };

        // Extract the class name from the category by dropping the namespace
        var category = entry.Category?.Split('.').Last() ?? string.Empty;
        listViewItem.SubItems.Add(category);

        // Extract batch and loaf index from the log entry's scope
        var scopes = entry.DeserialiseScopesJson();
        var batch = scopes.TryGetValue("BatchNumber", out var batchValue) ? batchValue : string.Empty;
        listViewItem.SubItems.Add(batch);

        var loafIndex = scopes.TryGetValue("LoafNumber", out var loafIndexValue) ? loafIndexValue : string.Empty;
        listViewItem.SubItems.Add(loafIndex);

        listViewItem.SubItems.Add(entry.RenderedMessage);

        // Set the background color based on the log level
        listViewItem.BackColor = entry.Level switch
        {
            LogLevel.Trace => Color.LightGray,
            LogLevel.Debug => Color.LightBlue,
            LogLevel.Information => Color.LightGreen,
            LogLevel.Warning => Color.Yellow,
            LogLevel.Error => Color.Orange,
            LogLevel.Critical => Color.Red,
            _ => Color.White,
        };
        listViewLogEntries.Items.Insert(0, listViewItem);

        // Auto-resize columns to fit the content
        foreach (ColumnHeader column in listViewLogEntries.Columns)
        {
            column.Width = -2; // -2 indicates auto-resize to fit the content
        }
    }

    /// <summary>
    /// Clears all log entries from the ListView and the cache.
    /// </summary>
    public void Clear()
    {
        logEntryUICache.Clear();
        listViewLogEntries.Items.Clear();
    }


    /// <summary>
    /// Gets the database IDs of the selected log entries.
    /// </summary>
    public long[] GetSelectedLogDbIds()
    {
        return listViewLogEntries.SelectedItems
            .Cast<ListViewItem>()
            .Where(item => item.Tag is LogEntry)
            .Select(item => ((LogEntry)item.Tag!).DbId)
            .ToArray();
    }
}
