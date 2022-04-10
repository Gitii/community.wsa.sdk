using System;

namespace Community.Wsa.Sdk.Exceptions;

/// <summary>
/// Specialized exception for WSA-service related errors.
/// </summary>
public class ServiceException : Exception
{
    /// <summary>
    /// Error code for this exception.
    /// </summary>
    public ServiceError Error { get; }

    /// <summary>
    /// Constructor for <see cref="ServiceException"/>.
    /// </summary>
    public ServiceException(ServiceError error, Exception? innerException = null)
        : base(GetErrorMessageFromCode(error), innerException)
    {
        Error = error;
    }

    private static string? GetErrorMessageFromCode(ServiceError error)
    {
        switch (error)
        {
            case ServiceError.CannotStartService:
                return "Cannot start the WSA-service";
            case ServiceError.CannotConnectToService:
                return "Adb cannot connect to WSA-service";
            default:
                throw new ArgumentOutOfRangeException(nameof(error), error, null);
        }
    }
}
