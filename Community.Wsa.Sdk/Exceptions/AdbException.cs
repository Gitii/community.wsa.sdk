using System;

namespace Community.Wsa.Sdk.Exceptions;

/// <summary>
/// Specialized exception for adb related errors.
/// </summary>
public class AdbException : Exception
{
    /// <summary>
    /// Error code for this exception.
    /// </summary>
    public AdbError Error { get; }

    /// <summary>
    /// Constructor for <see cref="AdbException"/>.
    /// </summary>
    public AdbException(AdbError error, Exception? innerException = null)
        : base(GetErrorMessageFromCode(error), innerException)
    {
        Error = error;
    }

    /// <summary>
    /// Constructor for <see cref="AdbException"/>.
    /// </summary>
    public AdbException(AdbError error, string additionalErrorMessage)
        : base(JoinMessages(GetErrorMessageFromCode(error), additionalErrorMessage))
    {
        Error = error;
    }

    /// <summary>
    /// Constructor for <see cref="AdbException"/>.
    /// </summary>
    public AdbException(AdbError error, string command, string additionalErrorMessage)
        : base(
            JoinMessages(
                GetErrorMessageFromCode(error),
                $"Command: {command}",
                additionalErrorMessage
            )
        )
    {
        Error = error;
    }

    private static string JoinMessages(params string[] parts)
    {
        return string.Join(Environment.NewLine, parts);
    }

    private static string GetErrorMessageFromCode(AdbError error)
    {
        switch (error)
        {
            case AdbError.AdbIsNotInstalled:
                return "Platform tools (which contains Adb executable) are not installed.";
            case AdbError.CannotStartAdb:
                return "Adb executable cannot be started by the system.";
            case AdbError.CommandFailed:
                return "Adb command could be started but the exit code wasn't zero.";
            case AdbError.CommandFinishedWithInvalidOutput:
                return "Adb command could be started and has finished but the output is invalid.";
            case AdbError.CommandTimedOut:
                return "Adb command has been started but timeout.";
            default:
                throw new ArgumentOutOfRangeException(nameof(error), error, null);
        }
    }
}
