using System.Diagnostics;

namespace ConsolePresenter;

/// <summary>
/// Synchronizes the current slide index between the main and notes process
/// via a .sync file in the slides folder + FileSystemWatcher.
/// </summary>
sealed class SlideSync : IDisposable
{
    private readonly string _filePath;
    private readonly FileSystemWatcher _watcher;
    private Action<int>? _onChanged;
    private int _lastWritten = -1;

    public static string GetSyncFilePath(string slidesDir) =>
        Path.Combine(slidesDir, ".sync");

    public SlideSync(string slidesDir)
    {
        _filePath = GetSyncFilePath(slidesDir);
        _watcher = new FileSystemWatcher(
            Path.GetDirectoryName(_filePath)!,
            Path.GetFileName(_filePath))
        {
            NotifyFilter = NotifyFilters.LastWrite,
            EnableRaisingEvents = false
        };
        _watcher.Changed += OnFileChanged;
    }

    /// <summary>Start listening. Callback is invoked on a thread pool thread.</summary>
    public void Start(Action<int> onSlideChanged)
    {
        _onChanged = onSlideChanged;
        _watcher.EnableRaisingEvents = true;
    }

    /// <summary>Write the current slide index to the sync file.</summary>
    public void Write(int slideIndex)
    {
        if (slideIndex == _lastWritten) return;
        _lastWritten = slideIndex;
        try
        {
            File.WriteAllText(_filePath, slideIndex.ToString());
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>Read the last written index (used at startup of the notes process).</summary>
    public int ReadCurrent()
    {
        try
        {
            if (File.Exists(_filePath) && int.TryParse(File.ReadAllText(_filePath), out var idx))
                return idx;
        }
        catch
        {
            // ignored
        }

        return -1;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        Thread.Sleep(50); // wait until the write is complete
        try
        {
            if (int.TryParse(File.ReadAllText(_filePath), out var idx) && idx != _lastWritten)
            {
                _lastWritten = idx;
                _onChanged?.Invoke(idx);
            }
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>Launch the notes window in a new terminal and return the process.</summary>
    public static Process? LaunchNotesWindow(string directory, int? maxMinutes = null)
    {
        var dll = typeof(SlideSync).Assembly.Location;
        var ownPid = Environment.ProcessId;
        var maxArg = maxMinutes.HasValue ? $" --max-minutes {maxMinutes.Value}" : string.Empty;
        string exeArgs, exe;

        if (string.IsNullOrEmpty(dll))
        {
            exe = Environment.ProcessPath!;
            exeArgs = $"\"{directory}\" --notes --parent {ownPid}{maxArg}";
        }
        else
        {
            exe = "dotnet";
            exeArgs = $"\"{dll}\" \"{directory}\" --notes --parent {ownPid}{maxArg}";
        }

        return Process.Start(new ProcessStartInfo("cmd.exe",
            $"/c start \"Slide Notes\" {exe} {exeArgs}")
        {
            UseShellExecute = true
        });
    }

    private string PidFilePath => Path.ChangeExtension(_filePath, ".pid");

    /// <summary>The notes process writes its PID on startup.</summary>
    public void WritePid() =>
        File.WriteAllText(PidFilePath, Environment.ProcessId.ToString());

    /// <summary>The main process reads the notes process's PID after it has been launched.</summary>
    public Process? ReadNotesProcess()
    {
        try
        {
            if (File.Exists(PidFilePath) && int.TryParse(File.ReadAllText(PidFilePath), out var pid))
                return Process.GetProcessById(pid);
        }
        catch
        {
            // ignored
        }

        return null;
    }

    public void Dispose()
    {
        _watcher.Dispose();
        try
        {
            File.Delete(PidFilePath);
        }
        catch
        {
            // ignored
        }
    }
}
