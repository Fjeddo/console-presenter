namespace ConsolePresenter;

internal sealed class NotesRenderer(PresentationTimer timer)
{
    public void Render(string? content, int slideNumber, int totalSlides, string title)
    {
        if (OperatingSystem.IsWindows())
        {
            Console.BufferHeight = Console.WindowHeight;
            Console.BufferWidth = Console.WindowWidth;
        }
        Console.Clear();

        // Header
        Console.ForegroundColor = ConsoleColor.Cyan;
        var timerText = timer.IsVisible ? timer.Elapsed : string.Empty;
        var header = $" Notes – Slide {slideNumber}/{totalSlides}: {title} ";
        var line = new string('─', Math.Max(0, Console.WindowWidth - 1));
        Console.WriteLine(line);
        var spaces = Math.Max(1, Console.WindowWidth - header.Length - timerText.Length);
        Console.Write(header + new string(' ', spaces));
        Console.ForegroundColor = timer.IsOverTime ? ConsoleColor.Red : ConsoleColor.Cyan;
        Console.WriteLine(timerText);
        Console.ForegroundColor = ConsoleColor.Cyan;
        if (timer.IsVisible) timer.StartLiveDisplay(row: 1);
        Console.WriteLine(line);
        Console.ResetColor();
        Console.WriteLine();

        if (string.IsNullOrWhiteSpace(content))
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  (No notes for this slide)");
            Console.ResetColor();
        }
        else
        {
            var wrapWidth = Math.Max(20, Console.WindowWidth - 5);
            foreach (var rawLine in content.Split('\n'))
            {
                var line2 = rawLine.TrimEnd('\r');
                if (line2.Length == 0)
                {
                    Console.WriteLine();
                    continue;
                }
                WriteWrapped(line2, wrapWidth);
            }
        }

        // Navigation bar at the bottom
        var navRow = Console.WindowHeight - 2;
        if (Console.CursorTop < navRow)
            Console.SetCursorPosition(0, navRow);

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(
            "  [← / ↑ Previous]  [→ / ↓ Next]  [0-9... Enter: jump]  [Q: quit]");
        Console.ResetColor();
    }

    private static void WriteWrapped(string text, int width)
    {
        var words = text.Split(' ');
        var current = new System.Text.StringBuilder();

        foreach (var word in words)
        {
            if (current.Length > 0 && current.Length + 1 + word.Length > width)
            {
                Console.WriteLine("  " + current.ToString().TrimEnd());
                current.Clear();
            }
            if (current.Length > 0) current.Append(' ');
            current.Append(word);
        }
        if (current.Length > 0)
            Console.WriteLine("  " + current);
    }
}
