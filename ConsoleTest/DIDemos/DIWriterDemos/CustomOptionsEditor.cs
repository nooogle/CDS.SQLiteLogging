using CDS.SQLiteLogging;
using Spectre.Console;

namespace ConsoleTest.DIWriterDemos;

/// <summary>
/// Interactive editor for batching and housekeeping options using Spectre.Console prompts.
/// </summary>
static class CustomOptionsEditor
{
    /// <summary>
    /// Prompts the user to edit batching options and returns the updated values.
    /// </summary>
    public static BatchingOptions GetBatchingOptions(BatchingOptions current)
    {
        AnsiConsole.Write(new Rule("[bold yellow]Batching Options[/]").LeftJustified());

        var currentGrid = new Grid().AddColumn(new GridColumn().NoWrap()).AddColumn();
        currentGrid.AddRow("[bold]Batch size:[/]", $"{current.BatchSize}");
        currentGrid.AddRow("[bold]Max cache size:[/]", $"{current.MaxCacheSize}");
        currentGrid.AddRow("[bold]Flush interval:[/]", $"{current.FlushInterval}");
        AnsiConsole.Write(new Panel(currentGrid).Header("[grey]Current[/]").Border(BoxBorder.Rounded));

        var batchSize = AnsiConsole.Prompt(
            new TextPrompt<int>("New [yellow]batch size[/]:")
                .DefaultValue(current.BatchSize)
                .Validate(v => v > 0
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Must be > 0[/]")));

        var cacheSize = AnsiConsole.Prompt(
            new TextPrompt<int>("New [yellow]max cache size[/]:")
                .DefaultValue(current.MaxCacheSize)
                .Validate(v => v > 0
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Must be > 0[/]")));

        var flushSecs = AnsiConsole.Prompt(
            new TextPrompt<int>("New [yellow]flush interval[/] (seconds):")
                .DefaultValue((int)current.FlushInterval.TotalSeconds)
                .Validate(v => v > 0
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Must be > 0[/]")));

        return new BatchingOptions
        {
            BatchSize = batchSize,
            MaxCacheSize = cacheSize,
            FlushInterval = TimeSpan.FromSeconds(flushSecs)
        };
    }

    /// <summary>
    /// Prompts the user to edit housekeeping options and returns the updated values.
    /// </summary>
    public static HouseKeepingOptions GetHouseKeepingOptions(HouseKeepingOptions current)
    {
        AnsiConsole.Write(new Rule("[bold yellow]Housekeeping Options[/]").LeftJustified());

        var currentGrid = new Grid().AddColumn(new GridColumn().NoWrap()).AddColumn();
        currentGrid.AddRow("[bold]Retention period:[/]", $"{current.RetentionPeriod.TotalDays:N0} days");
        currentGrid.AddRow("[bold]Cleanup interval:[/]", $"{current.CleanupInterval.TotalHours:N0} hours");
        AnsiConsole.Write(new Panel(currentGrid).Header("[grey]Current[/]").Border(BoxBorder.Rounded));

        var retentionDays = AnsiConsole.Prompt(
            new TextPrompt<int>("New [yellow]retention period[/] (days):")
                .DefaultValue((int)current.RetentionPeriod.TotalDays)
                .Validate(v => v > 0
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Must be > 0[/]")));

        var cleanupHours = AnsiConsole.Prompt(
            new TextPrompt<int>("New [yellow]cleanup interval[/] (hours):")
                .DefaultValue((int)current.CleanupInterval.TotalHours)
                .Validate(v => v > 0
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Must be > 0[/]")));

        return new HouseKeepingOptions
        {
            RetentionPeriod = TimeSpan.FromDays(retentionDays),
            CleanupInterval = TimeSpan.FromHours(cleanupHours),
        };
    }
}
