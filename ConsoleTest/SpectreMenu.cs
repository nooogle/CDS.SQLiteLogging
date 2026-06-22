using Spectre.Console;

namespace ConsoleTest;

/// <summary>
/// Helpers for building Spectre.Console-based interactive menus.
/// </summary>
static class SpectreMenu
{
    /// <summary>
    /// Shows a top-level looping menu with an "Exit" option.
    /// Returns when the user selects "Exit".
    /// </summary>
    public static void RunRoot(string title, params (string Label, Action Action)[] items)
        => RunLoop(title, "Exit", items);

    /// <summary>
    /// Shows a sub-level looping menu with a "Back" option.
    /// Returns when the user selects "Back".
    /// </summary>
    public static void Run(string title, params (string Label, Action Action)[] items)
        => RunLoop(title, "Back", items);

    private static void RunLoop(string title, string exitLabel, (string Label, Action Action)[] items)
    {
        while (true)
        {
            var choices = items.Select(i => i.Label).Append(exitLabel).ToList();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"\n[bold yellow]{Markup.Escape(title)}[/]")
                    .PageSize(15)
                    .HighlightStyle(new Style(foreground: Color.Yellow))
                    .AddChoices(choices));

            if (choice == exitLabel) { return; }

            var item = items.First(i => i.Label == choice);
            item.Action();
        }
    }
}
