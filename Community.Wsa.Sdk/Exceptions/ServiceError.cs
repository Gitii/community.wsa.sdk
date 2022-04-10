namespace Community.Wsa.Sdk.Exceptions;

/// <summary>
/// Enumeration of all errors for <see cref="ServiceException"/>.
/// </summary>
public enum ServiceError
{
    /// <summary>
    /// Indicates that the WSA service could not be started.
    /// </summary>
    CannotStartService,

    /// <summary>
    /// Indicates that adb could not connect to the running wsa service.
    /// This usually indicates that WSA developer mode hasn't been activated or that wsa service is not running (anymore).
    /// </summary>
    CannotConnectToService,

    /// <summary>
    /// Indicates that adb could not connect to the WSA device.
    /// </summary>
    CannotConnectToDevice,
}
