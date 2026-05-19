using System.Diagnostics;

namespace ConsolePresenter;

public sealed class PresentationTimer(int? maxMinutes = null)
{
    private Stopwatch _sw = Stopwatch.StartNew();
    private CancellationTokenSource? _cts;
    private readonly TimeSpan? _maxDuration = maxMinutes.HasValue ? TimeSpan.FromMinutes(maxMinutes.Value) : null;

    public string Elapsed => _sw.Elapsed.ToString(@"hh\:mm\:ss") + " ";
    public bool IsVisible { get; private set; }
    public bool IsOverTime => _maxDuration.HasValue && _sw.Elapsed > _maxDuration.Value;

    public void Toggle()
    {
        IsVisible = !IsVisible;
        if (!IsVisible) StopLiveDisplay();
    }

    public void Reset() => _sw = Stopwatch.StartNew();

    public void StartLiveDisplay(int row)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        Task.Run(async () =>
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(500, token);
                    var elapsed = Elapsed;
                    var col = Console.WindowWidth - elapsed.Length;
                    if (col < 0) continue;

                    var (origLeft, origTop) = Console.GetCursorPosition();
                    Console.SetCursorPosition(col, row);
                    Console.ForegroundColor = IsOverTime ? ConsoleColor.Red : ConsoleColor.Gray;
                    Console.Write(elapsed);
                    Console.ResetColor();
                    Console.SetCursorPosition(origLeft, origTop);
                }
            }
            catch (OperationCanceledException) { }
        }, token);
    }

    public void StopLiveDisplay() => _cts?.Cancel();
}
