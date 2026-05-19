using System.Diagnostics;
using System.Text;
using ConsolePresenter;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;
Console.CursorVisible = false;

// ── Argument parsing ──────────────────────────────────────────────────────
var notesMode = args.Contains("--notes");

var parentArgIdx = Array.IndexOf(args, "--parent");
Process? parentProcess = null;
if (parentArgIdx >= 0 && int.TryParse(args[parentArgIdx + 1], out var parentPid))
{
    try
    {
        parentProcess = Process.GetProcessById(parentPid);
    }
    catch
    {
        // ignored
    }
}

var maxMinutesArgIdx = Array.IndexOf(args, "--max-minutes");
int? maxMinutes = null;
if (maxMinutesArgIdx >= 0 && int.TryParse(args[maxMinutesArgIdx + 1], out var parsedMax))
{
    maxMinutes = parsedMax;
}

// Exclude indices that are known parameters or their values
var consumedIndices = new HashSet<int>();
if (parentArgIdx >= 0)
{
    consumedIndices.Add(parentArgIdx);
    consumedIndices.Add(parentArgIdx + 1);
}
if (maxMinutesArgIdx >= 0)
{
    consumedIndices.Add(maxMinutesArgIdx);
    consumedIndices.Add(maxMinutesArgIdx + 1);
}

var directory = args
    .Select((a, i) => (a, i))
    .FirstOrDefault(t => !t.a.StartsWith('-') && !consumedIndices.Contains(t.i))
    .a ?? Directory.GetCurrentDirectory();

// ── Load slides ───────────────────────────────────────────────────────────
var slides = SlideLoader.Load(directory);

if (slides.Count == 0)
{
    Console.WriteLine($"No slides found in: {directory}");
    Console.WriteLine("Files must follow the naming convention: 01. Title.txt, 02. Title.txt ...");
    return;
}

// ── Renderers ──────────────────────────────────────────────────────────────
var timer = new PresentationTimer(maxMinutes);
ISlideRenderer slideRenderer = new SpectreSlideRenderer(timer);
var notesRenderer = new NotesRenderer(timer);

// ── Synchronization ─────────────────────────────────────────────────────────
using var sync = new SlideSync(directory);
var current = 0;
var pendingSlide = -1;

sync.Start(idx =>
{
    if (idx >= 0 && idx < slides.Count)
        Interlocked.Exchange(ref pendingSlide, idx);
});

if (!notesMode)
{
    // Launch the notes window and write initial state
    SlideSync.LaunchNotesWindow(directory, maxMinutes);
    Thread.Sleep(500); // give the notes process time to start and write its PID
    var notesProcess = sync.ReadNotesProcess();
    RenderSlide();
    sync.Write(current);
    RunLoop(isNotes: false, partner: notesProcess);
}
else
{
    // Write own PID so that the main process can monitor us
    sync.WritePid();

    // Read any existing state from the main process
    var existing = sync.ReadCurrent();
    if (existing >= 0 && existing < slides.Count)
        current = existing;

    RenderNotes();
    RunLoop(isNotes: true, partner: parentProcess);
}

Console.Clear();
Console.CursorVisible = true;

// ── Local helper functions ─────────────────────────────────────────────────

void RenderSlide()
{
    slideRenderer.Render(
        File.ReadAllText(slides[current].Path),
        current + 1, slides.Count, slides[current].Title);
}

void RenderNotes()
{
    var notesPath = SlideLoader.GetNotesPath(slides[current].Path);
    var notesContent = notesPath != null ? File.ReadAllText(notesPath) : null;
    notesRenderer.Render(notesContent, current + 1, slides.Count, slides[current].Title);
}

void RunLoop(bool isNotes, Process? partner)
{
    var inputBuffer = string.Empty;

    while (true)
    {
        // Exit if the partner window has been closed
        if (partner?.HasExited == true) break;

        // Check if the other process has navigated
        var pending = Interlocked.Exchange(ref pendingSlide, -1);
        if (pending >= 0 && pending != current)
        {
            current = pending;
            if (isNotes) RenderNotes(); else RenderSlide();
            inputBuffer = string.Empty;
        }

        if (!Console.KeyAvailable)
        {
            Thread.Sleep(50);
            continue;
        }

        var key = Console.ReadKey(intercept: true);

        if (key.Key == ConsoleKey.Q) break;

        if (key.KeyChar == 'T')
        {
            timer.Toggle();
            if (isNotes) RenderNotes(); else RenderSlide();
            continue;
        }

        if (key.KeyChar == 'R')
        {
            timer.Reset();
            if (isNotes) RenderNotes(); else RenderSlide();
            continue;
        }

        var navigated = false;

        if (char.IsDigit(key.KeyChar))
        {
            inputBuffer += key.KeyChar;
            continue;
        }
        else if (key.Key == ConsoleKey.Enter && inputBuffer.Length > 0)
        {
            if (int.TryParse(inputBuffer, out var target))
            {
                var idx = slides.ToList().FindIndex(s => s.Number == target);
                if (idx >= 0) { current = idx; navigated = true; }
            }
            inputBuffer = string.Empty;
        }
        else
        {
            inputBuffer = string.Empty;

            if (key.Key is ConsoleKey.RightArrow or ConsoleKey.DownArrow)
            { current = Math.Min(current + 1, slides.Count - 1); navigated = true; }
            else if (key.Key is ConsoleKey.LeftArrow or ConsoleKey.UpArrow)
            { current = Math.Max(current - 1, 0); navigated = true; }
        }

        if (navigated)
        {
            if (isNotes) RenderNotes(); else RenderSlide();
            sync.Write(current);
        }
    }
}
