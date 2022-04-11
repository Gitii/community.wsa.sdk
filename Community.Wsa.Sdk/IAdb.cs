﻿using System.Net;
using System.Threading.Tasks;

namespace Community.Wsa.Sdk;

/// <summary>
/// Wrapper for the adb executable.
/// </summary>
public interface IAdb
{
    /// <summary>
    /// Checks whether adb.exe is installed. The folder that contains adb.exe must be added to <c>%PATH%</c>.
    /// </summary>
    bool IsInstalled { get; }

    /// <summary>
    /// Returns the full path to <c>adb.exe</c> or <c>null</c> if it's not installed.
    /// </summary>
    string? PathToAdb { get; }

    /// <summary>
    /// Installs the specified application package on the specified device.
    /// </summary>
    /// <param name="deviceSerialNumber">The serial number of the device on which the application will be installed.</param>
    /// <param name="filePath">The file path to the application package which will be installed.</param>
    /// <param name="allowDowngrade">If <c>true</c>, a downgrade is allowed, otherwise the installation will fail, if the application is already installed and has a higher version.</param>
    Task InstallPackageAsync(
        string deviceSerialNumber,
        string filePath,
        bool allowDowngrade = false
    );

    /// <summary>
    /// Uninstalls the specified package on the specified device.
    /// </summary>
    /// <param name="deviceSerialNumber">The serial number of the device on which the application will be installed.</param>
    /// <param name="packageName">The name of the package which will be uninstalled.</param>
    Task UninstallPackageAsync(string deviceSerialNumber, string packageName);

    /// <summary>
    /// Establishes a connection to any android system (device or emulator).
    /// </summary>
    /// <param name="endPoint">The endpoint of the system.</param>
    Task ConnectAsync(EndPoint endPoint);

    /// <summary>
    /// Lists all known android devices.
    /// </summary>
    /// <returns>List of known android devices.</returns>
    Task<KnownDevice[]> ListDevicesAsync();

    /// <summary>
    /// Returns a list of all (user) installed packages of the specified device.
    /// </summary>
    /// <param name="deviceSerialNumber">The serial number of the device which package list will be returned.</param>
    /// <returns>The list of installed packages.</returns>
    Task<PackageInfo[]> GetInstalledPackagesAsync(string deviceSerialNumber);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="deviceSerialNumber"></param>
    /// <param name="packageName"></param>
    /// <returns></returns>
    Task<PackageInfo?> GetInstalledPackageAsync(string deviceSerialNumber, string packageName);

    Task LaunchPackageAsync(string deviceSerialNumber, string packageName);

    Task DisconnectAsync(EndPoint endPoint);
}
