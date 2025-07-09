namespace CDS.SQLiteLogging;


/// <summary>
/// Provides a static, global middleware pipeline for log entries.
/// </summary>
public class LogPipeline
{
    /// <summary>
    /// The list of middlewares that will process log entries.
    /// </summary>
    private readonly List<ILogMiddleware> middlewares;


    /// <summary>
    /// Initializes a new instance of the <see cref="LogPipeline"/> class with the specified middlewares.
    /// </summary>
    public LogPipeline(IEnumerable<ILogMiddleware> middlewares)
    {
        this.middlewares = middlewares.ToList();
    }


    /// <summary>
    /// Executes the log pipeline with the provided log entry data and a final handler.
    /// </summary>
    /// <param name="data">
    /// The log entry data to be processed by the pipeline.
    /// </param>
    /// <param name="finalHandler">
    /// The final handler to be called after all middlewares have processed the log entry.
    /// </param>
    /// <returns></returns>
    public async Task ExecuteAsync(LogEntry data)
    {
        var enumerator = middlewares.GetEnumerator();

        Task Next()
        {
            if (!enumerator.MoveNext())
            {
                return Task.CompletedTask;
            }

            var current = enumerator.Current;
            return current.InvokeAsync(data, Next);
        }

        await Next();
    }
}
