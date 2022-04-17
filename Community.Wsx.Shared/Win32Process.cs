using System.Diagnostics;

namespace Community.Wsx.Shared;

internal class Win32Process : IProcess
{
    private readonly Process _process;

    public Win32Process(Process process)
    {
        _process = process;
    }

    public void Dispose()
    {
        _process.Dispose();
    }

    public bool HasExited => _process.HasExited;

    public StreamReader StandardOutput => _process.StandardOutput;

    public StreamReader StandardError => _process.StandardError;

    public StreamWriter StandardInput => _process.StandardInput;

    public bool EnableRaisingEvents
    {
        get => _process.EnableRaisingEvents;
        set => _process.EnableRaisingEvents = value;
    }

    public event EventHandler? Exited
    {
        add => _process.Exited += value;
        remove => _process.Exited -= value;
    }

    public int ExitCode => _process.ExitCode;

    public void WaitForExit()
    {
        _process.WaitForExit();
    }

    public async Task<int> WaitForExitAsync()
    {
        await _process.WaitForExitAsync().ConfigureAwait(false);

        return _process.ExitCode;
    }

    public async Task<int?> WaitForExitAsync(int milliseconds)
    {
        using CancellationTokenSource src = new CancellationTokenSource(milliseconds);

        try
        {
            await _process.WaitForExitAsync(src.Token).ConfigureAwait(false);

            if (_process.HasExited)
            {
                return _process.ExitCode;
            }

            return null;
        }
        catch (TaskCanceledException)
        {
            return null;
        }
    }

    public void Kill()
    {
        _process.Kill();
    }
}
