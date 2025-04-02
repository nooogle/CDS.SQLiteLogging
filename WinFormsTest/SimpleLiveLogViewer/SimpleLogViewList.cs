using CDS.SQLiteLogging;
using Humanizer;

namespace WinFormsTest.SimpleListLogViewer;

public partial class SimpleLogViewList : UserControl
{
    private LogEntryUICache logEntryUICache;


    public int MaxQueueSize
    {
        get => logEntryUICache.MaxQueueSize;
        set => logEntryUICache.MaxQueueSize = value;
    }


    public Func<LogEntry, bool>? Filter
    {
        get => logEntryUICache.Filter;
        set => logEntryUICache.Filter = value;
    }


    public SimpleLogViewList()
    {
        InitializeComponent();
        logEntryUICache = new LogEntryUICache();
    }


    public void QueueLogEntry(LogEntry entry)
    {
        logEntryUICache.Add(entry);
    }

    private void timerGrabPendingLogEntries_Tick(object sender, EventArgs e)
    {
        var cachedLogEntries = logEntryUICache.DequeueAll();
        if(cachedLogEntries == null)
        {
            return;
        }

        foreach (var entry in cachedLogEntries)
        {
            var localTime = entry.Timestamp.ToLocalTime();

            var listViewItem = new ListViewItem(entry.Level.Humanize());
            
            listViewItem.Tag = entry;
            listViewItem.SubItems.Add(localTime.ToString("G"));
            listViewItem.SubItems.Add(entry.RenderedMessage);

            listViewLogEntries.Items.Insert(0, listViewItem);
        }
    }

    public void Clear()
    {
        logEntryUICache.Clear();
        listViewLogEntries.Items.Clear();
    }
}
