namespace CDS.SQLiteLogging;

/// <summary>
/// Builder class for constructing a log pipeline with middlewares.
/// </summary>
public class LogPipelineBuilder
{
    /// <summary>
    /// The list of middlewares that will process log entries.
    /// </summary>
    private readonly List<ILogMiddleware> middlewares = new List<ILogMiddleware>();


    /// <summary>
    /// Represents an empty log pipeline builder instance.
    /// </summary>
    public static LogPipelineBuilder Empty => new LogPipelineBuilder();


    /// <summary>
    /// Adds a middleware to the log pipeline using a fluent interface.
    /// </summary>
    /// <param name="middleware">
    /// The middleware to be added to the pipeline. This should implement the <see cref="ILogMiddleware"/> interface.
    /// </param>
    /// <returns>
    /// Returns the current instance of <see cref="LogPipeline"/> to allow for method chaining.
    /// </returns>
    public LogPipelineBuilder Add(ILogMiddleware middleware)
    {
        middlewares.Add(middleware);
        return this;
    }


    /// <summary>
    /// Builds the log pipeline with the configured middlewares.
    /// </summary>
    public LogPipeline Build()
    {
        return new LogPipeline(middlewares);
    }
}
