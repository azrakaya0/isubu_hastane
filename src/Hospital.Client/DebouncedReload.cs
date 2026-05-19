namespace Hospital.Client;

/// <summary>
/// Liste filtrelerinde yazarken gecikmeli yenileme; Enter veya anında tetikleme için <see cref="RunNowAsync"/>.
/// </summary>
internal sealed class DebouncedReload
{
    private readonly Func<Task> _reload;
    private readonly int _delayMs;
    private CancellationTokenSource? _cts;

    public DebouncedReload(Func<Task> reload, int delayMs = 450)
    {
        _reload = reload;
        _delayMs = delayMs;
    }

    public void Schedule()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        _ = RunDelayedAsync(token);
    }

    public async Task RunNowAsync()
    {
        _cts?.Cancel();
        await _reload();
    }

    private async Task RunDelayedAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(_delayMs, token);
            if (!token.IsCancellationRequested)
            {
                await _reload();
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}
