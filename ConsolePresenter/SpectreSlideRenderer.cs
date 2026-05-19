using Spectre.Console;

namespace ConsolePresenter;

public sealed class SpectreSlideRenderer(PresentationTimer timer) : ISlideRenderer
{
    // Rule(1) + panel-border-top(1) + panel-padding-top(1)
    // + panel-padding-bottom(1) + panel-border-bottom(1) + nav(1) - border fills width → auto-wrap
    private const int ReservedLines = 5;

    public void Render(string content, int slideNumber, int totalSlides, string title)
    {
        if (OperatingSystem.IsWindows())
        {
            Console.BufferHeight = Console.WindowHeight;
            Console.BufferWidth = Console.WindowWidth;
        }
        AnsiConsole.Clear();

        // Header
        var timerText = timer.IsVisible ? timer.Elapsed : string.Empty;
        var timerColor = timer.IsOverTime ? "red" : "grey";
        var slideInfo = Markup.Escape(title);
        var slideNum = $" {slideNumber.ToString().PadLeft(totalSlides.ToString().Length)}/{totalSlides}";
        var width = Console.WindowWidth - 1;
        var plainLength = slideNum.Length + title.Length + timerText.Length;
        var spaces = Math.Max(1, width - plainLength);
        AnsiConsole.MarkupLine($"[grey]{slideNum}[/] [Cyan]{slideInfo}[/]{new string(' ', spaces)}[{timerColor}]{timerText}[/]");
        if (timer.IsVisible) timer.StartLiveDisplay(row: 0);

        // Pad/truncate
        var contentLines = Console.WindowHeight - ReservedLines;
        var lines = content.Split('\n');
        var paddedLines = new string[contentLines];
        for (var i = 0; i < contentLines; i++)
            paddedLines[i] = i < lines.Length ? lines[i].TrimEnd('\r') : string.Empty;
        var fixedContent = string.Join('\n', paddedLines);

        // Convert \[ / \] (backslash-escape) to [[ / ]] (Spectre.Console-escape)
        var escapedContent = fixedContent.Replace("\\[", "[[").Replace("\\]", "]]");
        try
        {
            var panel = new Panel(escapedContent)
            {
                Border = BoxBorder.Rounded,
                Padding = new Padding(2, 1),
                Expand = true,
            };
            AnsiConsole.Write(panel);
        }
        catch (InvalidOperationException)
        {
            var fallbackPanel = new Panel(Markup.Escape(fixedContent))
            {
                Border = BoxBorder.Rounded,
                Padding = new Padding(2, 1),
                Expand = true,
            };
            AnsiConsole.Write(fallbackPanel);
        }

        // Navigation bar
        AnsiConsole.Markup(
            " [grey][[ [grey]← / ↑[/] Previous ]]  [[ [grey]→ / ↓[/] Next ]]  " +
            "[[ [grey]0-9... Enter[/]: jump ]]  [[ [grey]Q[/]: quit ]][/]");
    }
}
