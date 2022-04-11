using System;
using System.Threading.Tasks;
using Community.Wsx.Shared;

namespace Community.Wsa.Sdk;

/// <inheritdoc />
public class AdbPackageManager : IPackageManager
{
    private readonly IAdb _adb;

    public AdbPackageManager(
        IEnvironment? environment = null,
        IProcessManager? processManager = null,
        IIo? io = null,
        IAdb? adb = null
    )
    {
        _adb = adb ?? new AdbClient(processManager, environment, io);
    }

    /// <inheritdoc />
    public Task InstallPackageAsync(
        string deviceId,
        string filePath,
        IProgress<string>? progress = null
    )
    {
        return _adb.InstallPackageAsync(deviceId, filePath);
    }

    /// <inheritdoc />
    public Task<PackageInfo[]> GetAllInstalledPackagesAsync(string deviceId)
    {
        return _adb.GetInstalledPackagesAsync(deviceId);
    }

    /// <inheritdoc />
    public Task<PackageInfo?> GetInstalledPackageAsync(string deviceId, string packageName)
    {
        return _adb.GetInstalledPackageAsync(deviceId, packageName);
    }

    /// <inheritdoc />
    public async Task<bool> IsPackageInstalledAsync(string deviceId, string packageName)
    {
        return (
                await GetInstalledPackageAsync(deviceId, packageName).ConfigureAwait(false)
            )?.PackageName == packageName;
    }

    /// <inheritdoc />
    public Task UninstallPackageAsync(string deviceId, string packageName)
    {
        return _adb.UninstallPackageAsync(deviceId, packageName);
    }

    /// <inheritdoc />
    public Task LaunchAsync(string deviceId, string packageName)
    {
        return _adb.LaunchPackageAsync(deviceId, packageName);
    }
}
