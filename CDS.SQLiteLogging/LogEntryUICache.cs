using System.Collections.Concurrent;

namespace CDS.SQLiteLogging;


/// <summary>
/// Provides a thread-safe cache for log entries intended for UI display.
/// </summary>
public class LogEntryUICache
{
    private readonly ConcurrentQueue<LogEntry> queue = new();
    private int maxQueueSize;
    private Func<LogEntry, bool>? filter;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogEntryUICache"/> class.
    /// </summary>
    /// <param name="maxQueueSize">The maximum number of entries to store in the queue. The default is 1000</param>
    /// <param name="filter">Optional filter to determine if an entry should be added. If null, all entries are added.</param>
    public LogEntryUICache(int maxQueueSize = 1000, Func<LogEntry, bool>? filter = null)
    {
        MaxQueueSize = maxQueueSize;
        Filter = filter ?? (_ => true);
    }

    /// <summary>
    /// Gets or sets the maximum number of entries allowed in the queue.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the value is less than or equal to zero.</exception>
    public int MaxQueueSize
    {
        get => maxQueueSize;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentException("Max queue size must be greater than zero.", nameof(value));
            }
            maxQueueSize = value;
        }
    }

    /// <summary>
    /// Gets or sets the filter used to determine which entries are added to the queue.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
    public Func<LogEntry, bool>? Filter
    {
        get => filter;
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), "Filter cannot be null.");
            }
            filter = value;
        }
    }

    /// <summary>
    /// Gets the current number of entries in the queue.
    /// </summary>
    public int Count => queue.Count;

    /// <summary>
    /// Adds a log entry to the queue if it passes the filter and the queue is not full.
    /// </summary>
    /// <param name="entry">The log entry to add.</param>
    public void Add(LogEntry entry)
    {
        if (entry == null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        if (!filter!(entry))
        {
            return;
        }

        // If we've reached max capacity, don't add more
        if (queue.Count >= maxQueueSize)
        {
            return;
        }

        queue.Enqueue(entry);
    }

    /// <summary>
    /// Dequeues all available log entries.
    /// </summary>
    /// <returns>A list of dequeued log entries.</returns>
    public List<LogEntry> DequeueAll()
    {
        var result = new List<LogEntry>();

        while (queue.TryDequeue(out var entry))
        {
            result.Add(entry);
        }

        return result;
    }

    /// <summary>
    /// Clears all entries from the queue.
    /// </summary>
    public void Clear()
    {
        while (queue.TryDequeue(out _))
        {
            // Empty block since we're just discarding entries
        }
    }
}
