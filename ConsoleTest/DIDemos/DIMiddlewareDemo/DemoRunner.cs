using CDS.SQLiteLogging.MEL;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace ConsoleTest.DIMiddlewareDemo;

/// <summary>
/// Demonstrates the middleware pipeline and global log context in a DI scenario.
/// Writes log entries tagged with an ambient AppId, then reads them back and
/// displays each entry's context values in a table.
/// </summary>
class DemoRunner
{
    /// <summary>
    /// Runs the demo.
    /// </summary>
    public static void Run()
    {
        AnsiConsole.Write(new Rule("[bold yellow]Middleware &amp; Global Context Demo[/]").LeftJustified());

        string dbPath = DBPathCreator.Create();

        WriteLogEntries(dbPath);
        ReadBackLogEntries(dbPath);
    }

    private static ServiceProvider WriteLogEntries(string dbPath)
    {
        var logPipeline =
            CDS.SQLiteLogging.LogPipelineBuilder.Empty
            .Add(new CDS.SQLiteLogging.GlobalLogContextMiddleware())
            .Build();

        CDS.SQLiteLogging.GlobalLogContext.Set(
            key: GlobalLogContextKeys.AppId,
            value: $"{DateTime.Now.Ticks}");

        var sqliteLoggerProvider = MELLoggerProvider.Create(fileName: dbPath, logPipeline: logPipeline);

        var serviceProvider = new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddProvider(sqliteLoggerProvider);
                builder.SetMinimumLevel(LogLevel.Trace);
            })
            .AddTransient<DemoService>()
            .BuildServiceProvider();

        AnsiConsole.Status().Start("Running demo service...", _ =>
        {
            serviceProvider.GetRequiredService<DemoService>().RunAsync().Wait();
        });

        sqliteLoggerProvider.LoggerUtilities.WaitUntilCacheIsEmpty(TimeSpan.FromSeconds(5));

        return serviceProvider;
    }

    private static void ReadBackLogEntries(string dbPath)
    {
        using var sqliteReader = new CDS.SQLiteLogging.Reader(dbPath);

        var allEntries = sqliteReader.GetAllEntries();

        AnsiConsole.MarkupLine($"Entries written: [bold]{allEntries.Count:N0}[/]\n");

        var table = new Table()
            .AddColumn("[bold]App ID[/]")
            .AddColumn("[bold]Batch[/]")
            .AddColumn("[bold]Level[/]")
            .AddColumn("[bold]Message[/]")
            .Border(TableBorder.Rounded);

        foreach (var entry in allEntries)
        {
            table.AddRow(
                Markup.Escape(GetProperty(entry, GlobalLogContextKeys.AppId).TruncateLeft(12)),
                Markup.Escape(GetProperty(entry, GlobalLogContextKeys.BatchNumber)),
                LevelMarkup(entry.Level),
                Markup.Escape(entry.RenderedMessage));
        }

        AnsiConsole.Write(table);
    }

    private static string GetProperty(CDS.SQLiteLogging.LogEntry logEntry, string key)
    {
        if (logEntry.Properties != null && logEntry.Properties.TryGetValue(key, out var value))
        {
            return value?.ToString() ?? string.Empty;
        }
        return string.Empty;
    }

    private static string LevelMarkup(LogLevel level) => level switch
    {
        LogLevel.Trace => "[grey]Trace[/]",
        LogLevel.Debug => "[grey]Debug[/]",
        LogLevel.Information => "[green]Info[/]",
        LogLevel.Warning => "[yellow]Warn[/]",
        LogLevel.Error => "[red bold]Error[/]",
        LogLevel.Critical => "[red bold on white]CRIT[/]",
        _ => Markup.Escape(level.ToString())
    };
}

/// <summary>
/// Extension to truncate a string from the left, keeping the tail.
/// </summary>
file static class StringExtensions
{
    public static string TruncateLeft(this string s, int maxLength)
        => s.Length <= maxLength ? s : "…" + s.Substring(s.Length - (maxLength - 1));
}
