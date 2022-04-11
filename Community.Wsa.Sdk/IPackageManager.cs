using System;
using System.Threading.Tasks;

namespace Community.Wsa.Sdk;

/// <summary>
/// Manages apps.
/// </summary>
public interface IPackageManager
{
    /// <summary>
    /// Install a new or upgrade an existing package.
    /// </summary>
    /// <param name="deviceId">Use device with given serial</param>
    /// <param name="filePath">File path to the app package.</param>
    /// <param name="progress">Optional provider for reporting installation updates.</param>
    public Task InstallPackageAsync(
        string deviceId,
        string filePath,
        IProgress<string>? progress = null
    );

    /// <summary>
    /// Returns all installed packages.
    /// </summary>
    /// <param name="deviceId">Use device with given serial</param>
    public Task<PackageInfo[]> GetAllInstalledPackagesAsync(string deviceId);

    /// <summary>
    /// Gets package info of the specified installed package.
    /// </summary>
    /// <param name="deviceId">Use device with given serial</param>
    /// <param name="packageName">Name of the package which info should be fetched.</param>
    /// <returns>Info of the installed package or <c>null</c> if the package isn't installed.</returns>
    public Task<PackageInfo?> GetInstalledPackageAsync(string deviceId, string packageName);

    /// <summary>
    /// Checks whether the specified package is installed.
    /// </summary>
    /// <param name="deviceId">Use device with given serial</param>
    /// <param name="packageName">The name of the package which will be checked.</param>
    /// <returns><c>true</c> if the package is installed, otherwise <c>false</c>.</returns>
    public Task<bool> IsPackageInstalledAsync(string deviceId, string packageName);

    /// <summary>
    /// Uninstalls the specified package.
    /// </summary>
    /// <param name="deviceId">Use device with given serial</param>
    /// <param name="packageName">The name of the package which will be uninstalled.</param>
    public Task UninstallPackageAsync(string deviceId, string packageName);

    /// <summary>
    /// Launches the installed package.
    /// </summary>
    /// <param name="deviceId">Use device with given serial</param>
    /// <param name="packageName">The name of the package which will be launched.</param>
    public Task LaunchAsync(string deviceId, string packageName);
}
