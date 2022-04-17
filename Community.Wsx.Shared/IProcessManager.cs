using System.Diagnostics;

namespace Community.Wsx.Shared;

/// <summary>
/// Wrapper for <see cref="Process.Start(System.Diagnostics.ProcessStartInfo)"/>.
/// Returns an <see cref="Process"/> wrapped in a <see cref="IProcess"/>.
/// </summary>
public interface IProcessManager
{
    /// <summary>
    /// <inheritdoc cref="Process.Start(System.Diagnostics.ProcessStartInfo)"/>
    /// </summary>
    /// <param name="startInfo"><inheritdoc cref="Process.Start(System.Diagnostics.ProcessStartInfo)" path="/param[@name='startInfo']"/></param>
    /// <returns></returns>
    public abstract IProcess? Start(ProcessStartInfo startInfo);

    /// <summary>
    /// Find and returns the first process of that program (identified by <paramref cref="programName"/>).
    /// </summary>
    public IProcess? Find(string programName);
}
