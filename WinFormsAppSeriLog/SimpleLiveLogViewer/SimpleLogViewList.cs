using Humanizer;
using Serilog.Events;

namespace WinFormsAppSeriLog.SimpleListLogViewer;

public partial class SimpleLogViewList : UserControl
{
    private Utils.LogEntryUICache logEntryUICache;


    public int MaxQueueSize
    {
        get => logEntryUICache.MaxQueueSize;
        set => logEntryUICache.MaxQueueSize = value;
    }


    public Func<LogEvent, bool>? Filter
    {
        get => logEntryUICache.Filter;
        set => logEntryUICache.Filter = value;
    }


    public SimpleLogViewList()
    {
        InitializeComponent();
        logEntryUICache = new Utils.LogEntryUICache();
    }


    private void timerGrabPendingLogEntries_Tick(object sender, EventArgs e)
    {
        var cachedLogEntries = logEntryUICache.DequeueAll();
        if (cachedLogEntries == null)
        {
            return;
        }

        foreach (var evt in cachedLogEntries)
        {
            var localTime = evt.Timestamp.ToLocalTime();
            var listViewItem = new ListViewItem("0");
            listViewItem.Tag = evt;
            listViewItem.SubItems.Add(evt.Level.Humanize());
            listViewItem.SubItems.Add(localTime.ToString("G"));
            listViewItem.SubItems.Add(evt.RenderMessage());
            //listViewItem.SubItems.Add(evt.Exception?.Message ?? string.Empty);
            //listViewItem.SubItems.Add(evt.Exception?.StackTrace ?? string.Empty);
            listViewLogEntries.Items.Insert(0, listViewItem);
        }
    }

    public void Clear()
    {
        logEntryUICache.Clear();
        listViewLogEntries.Items.Clear();
    }

    public void QueueLogEntry2(LogEvent evt)
    {
        if (evt == null)
        {
            return;
        }

        logEntryUICache.Add(evt);   
    }
}
