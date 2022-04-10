using System.Diagnostics;

namespace Community.Wsx.Shared;

/// <summary>
/// Wrapper for a <see cref="Process"/>.
/// <inheritdoc cref="Process"/>
/// </summary>
public interface IProcess : IDisposable
{
    /// <inheritdoc cref="Process.HasExited"/>
    public bool HasExited { get; }

    /// <inheritdoc cref="Process.StandardOutput"/>
    public StreamReader StandardOutput { get; }

    /// <inheritdoc cref="Process.StandardError"/>
    public StreamReader StandardError { get; }

    /// <inheritdoc cref="Process.StandardInput"/>
    public StreamWriter StandardInput { get; }

    /// <inheritdoc cref="Process.EnableRaisingEvents"/>
    public bool EnableRaisingEvents { get; set; }

    /// <inheritdoc cref="Process.Exited"/>
    public event EventHandler Exited;

    /// <inheritdoc cref="Process.ExitCode"/>
    public int ExitCode { get; }

    /// <inheritdoc cref="Process.WaitForExit()"/>
    public void WaitForExit();

    /// <summary>
    /// Async version of <seealso cref="Process.WaitForExit()"/>
    /// </summary>
    Task<int> WaitForExitAsync();

    /// <summary>
    /// Async version of <seealso cref="Process.WaitForExit(int)"/>
    /// </summary>
    Task<int?> WaitForExitAsync(int milliseconds);

    /// <inheritdoc cref="Process.Kill()"/>
    public void Kill();
}
