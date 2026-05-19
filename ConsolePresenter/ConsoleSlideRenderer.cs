namespace ConsolePresenter;

sealed class ConsoleSlideRenderer : ISlideRenderer
{
    // Number of lines reserved for the header (1), blank line (1) and navigation bar (1)
    private const int ReservedLines = 3;

    public void Render(string content, int slideNumber, int totalSlides, string title)
    {
        // On Windows the buffer must match the window, otherwise Clear scrolls upward
        if (OperatingSystem.IsWindows())
        {
            Console.BufferHeight = Console.WindowHeight;
            Console.BufferWidth = Console.WindowWidth;
        }
        Console.Clear();

        var contentLines = Console.WindowHeight - ReservedLines;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"─── Slide {slideNumber}/{totalSlides}: {title} ───");
        Console.ResetColor();
        Console.WriteLine();

        var lines = content.Split('\n');
        for (var i = 0; i < contentLines; i++)
        {
            Console.WriteLine(i < lines.Length ? lines[i].TrimEnd('\r') : string.Empty);
        }

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("[ ← / ↑ Previous ]  [ → / ↓ Next ]  [ 0-9... Enter: jump ]  [ Q: quit ]");
        Console.ResetColor();
    }
}
