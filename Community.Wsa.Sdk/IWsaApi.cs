using System;
using System.Threading.Tasks;

namespace Community.Wsa.Sdk;

/// <summary>
/// Provides functionality to help you call WSA from .NET applications.
/// </summary>
public interface IWsaApi
{
    /// <summary>
    /// Checks if the environment, you are running in, supports WSA.
    /// </summary>
    public bool IsWsaSupported()
    {
        return IsWsaSupported(out _);
    }

    /// <summary>
    /// Checks if the environment you are running in now supports WSA.
    /// The error message is returned as out parameter. If WSA is supported, <paramref name="missingCapabilities"/> is <c>null</c>.
    /// </summary>
    public bool IsWsaSupported(out string? missingCapabilities);

    /// <summary>
    /// Returns <c>true</c> when WSA is running, otherwise <c>false</c>.
    /// </summary>
    public bool IsRunning { get; }

    /// <summary>
    /// Returns <c>true</c> when WSA is installed, otherwise <c>false</c>.
    /// </summary>
    public bool IsWsaInstalled { get; }

    /// <summary>
    /// Ensures that WSA is ready (started and adb is connected).
    /// </summary>
    /// <param name="progress">Provider for progress updates.</param>
    public Task EnsureWsaIsReadyAsync(IProgress<string>? progress = null);

    /// <summary>
    /// Starts the WSA service if it's not already running.
    /// </summary>
    public Task StartServiceAsync();

    /// <summary>
    /// Returns the device serial number of the WSA device.
    /// Will fail if the service is not running or adb is not connected.
    /// Use <see cref="EnsureWsaIsReadyAsync"/> to prepare wsa beforehand.
    /// </summary>
    /// <returns>The serial number of the wsa device.</returns>
    public Task<string> GetDeviceIdAsync();

    public const string ADB_WSA_DEVICE_SERIAL_NUMBER = "localhost:58526";
    public const string WSA_MUTEX = "{42CEB0DF-325A-4FBE-BBB6-C259A6C3F0BB}";
    public const string WSA_HOST_NAME = "localhost";
    public const int WSA_PORT = 58526;
    public const string WSA_MODEL_NUMBER = "Subsystem_for_Android_TM_";
}
