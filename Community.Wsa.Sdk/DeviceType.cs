namespace Community.Wsa.Sdk;

/// <summary>
/// Enumeration of known device types.
/// </summary>
public enum DeviceType
{
    /// <summary>
    /// Device is offline. The real type is unknown.
    /// </summary>
    Offline,

    /// <summary>
    /// The device is a physical device.
    /// </summary>
    Device,

    /// <summary>
    /// The device is running in an emulator.
    /// </summary>
    Emulator,

    /// <summary>
    /// The device access is unauthorized.
    /// </summary>
    Unauthorized
}
