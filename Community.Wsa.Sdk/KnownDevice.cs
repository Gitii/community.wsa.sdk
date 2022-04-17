namespace Community.Wsa.Sdk;

/// <summary>
/// Describes a device known by adb.
/// </summary>
public readonly struct KnownDevice
{
    /// <summary>
    /// The serial number of the device.
    /// </summary>
    public string DeviceSerialNumber { get; init; }

    /// <summary>
    /// Specifies whether a device is offline or online/connected.
    /// </summary>
    public bool IsOffline => DeviceType == DeviceType.Offline;

    /// <summary>
    /// Specifies whether the connected device is a physical device (nor a emulator nor offline).
    /// </summary>
    public bool IsDevice => DeviceType == DeviceType.Device;

    /// <summary>
    /// Specifies whether the connected device is an emulator (nor a physical device nor offline)
    /// </summary>
    public bool IsEmulator => DeviceType == DeviceType.Emulator;

    /// <summary>
    /// The device type.
    /// </summary>
    public DeviceType DeviceType { get; init; }

    /// <summary>
    /// Product code of the device
    /// </summary>
    public string ProductCode { get; init; }

    /// <summary>
    /// Model number of the device
    /// </summary>
    public string ModelNumber { get; init; }

    /// <summary>
    /// Device code of the device
    /// </summary>
    public string DeviceCode { get; init; }

    /// <summary>
    /// Transport Id of the device
    /// </summary>
    public string TransportId { get; init; }
}
