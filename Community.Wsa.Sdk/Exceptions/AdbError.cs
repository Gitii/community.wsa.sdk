namespace Community.Wsa.Sdk.Exceptions;

/// <summary>
/// Enumeration of all errors for <see cref="AdbException"/>.
/// </summary>
public enum AdbError
{
    /// <summary>
    /// Indicates that platform tools (which contains the adb executable) aren't installed or not added to the PATH-environment variable.
    /// </summary>
    AdbIsNotInstalled,

    /// <summary>
    /// Indicates that the adb executable cannot be started by the system (for example invalid executable or permission issues).
    /// </summary>
    CannotStartAdb,

    /// <summary>
    /// Indicates that the adb command could be started but the exit code wasn't zero.
    /// </summary>
    CommandFailed,

    /// <summary>
    /// Indicates that the adb command could be started but the actual output hasn't matched the expected output.
    /// </summary>
    CommandFinishedWithInvalidOutput,

    /// <summary>
    /// Indicates that the adb command could be started successfully but took to long to complete.
    /// </summary>
    CommandTimedOut
}
