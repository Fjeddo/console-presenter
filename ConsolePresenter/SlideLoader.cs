namespace ConsolePresenter;

static class SlideLoader
{
    /// <summary>
    /// Returns files sorted by their numeric prefix (01, 02, 03 ...).
    /// Naming convention: "01. Title.txt", "02. Title.txt" etc.
    /// </summary>
    public static IReadOnlyList<SlideFile> Load(string directory)
    {
        var files = Directory.EnumerateFiles(directory, "*.txt")
            .Select(path =>
            {
                var name = Path.GetFileNameWithoutExtension(path);
                var dotIndex = name.IndexOf('.');
                if (dotIndex > 0 && int.TryParse(name[..dotIndex].Trim(), out var number))
                {
                    var title = name[(dotIndex + 1)..].Trim();
                    return new SlideFile(number, title, path);
                }
                return null;
            })
            .OfType<SlideFile>()
            .OrderBy(s => s.Number)
            .ToList();

        return files;
    }

    /// <summary>
    /// Attempts to find a notes file for the given slide.
    /// Notes files reside in the same directory as the slides with the .notes extension,
    /// e.g. "01. Agenda.notes" next to "01. Agenda.txt".
    /// </summary>
    public static string? GetNotesPath(string slidePath) =>
        Path.ChangeExtension(slidePath, ".notes") is { } p && File.Exists(p) ? p : null;
}

record SlideFile(int Number, string Title, string Path);
