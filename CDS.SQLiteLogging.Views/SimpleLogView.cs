using CDS.SQLiteLogging;
using Humanizer;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace CDS.SQLiteLogging.Views;

/// <summary>
/// A user control that displays live log entries.
/// </summary>
public partial class SimpleLogView : UserControl
{
    private readonly LogEntryUICache logEntryUICache;

    /// <summary>
    /// Gets or sets the maximum queue size for log entries.
    /// </summary>
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
    /// Initializes a new instance of the <see cref="SimpleLogView"/> class.
    /// </summary>
    public SimpleLogView()
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

        var listViewItem = new ListViewItem(entry.DbId.ToString())
        {
            Tag = entry
        };

        // Add timestamp, category, and message to the ListViewItem
        listViewItem.SubItems.Add(localTime.ToString("T"));
        listViewItem.SubItems.Add(entry.Category ?? string.Empty);
        listViewItem.SubItems.Add(entry.RenderedMessage);

        // Add scope info
        listViewItem.SubItems.Add(entry.ScopesJson ?? string.Empty);


        //// Extract batch and loaf index from the log entry's scope
        //var scopes = entry.DeserialiseScopesJson();
        //var batch = scopes.TryGetValue("BatchNumber", out var batchValue) ? batchValue : string.Empty;
        //listViewItem.SubItems.Add(batch);

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
    /// Gets the IDs of the selected log entries in the ListView.
    /// </summary>
    public long[] GetSelectedLogIDs()
    {
        return listViewLogEntries.SelectedItems
            .Cast<ListViewItem>()
            .Where(item => item.Tag is LogEntry)
            .Select(item => ((LogEntry)item.Tag!).DbId)
            .ToArray();
    }
}
