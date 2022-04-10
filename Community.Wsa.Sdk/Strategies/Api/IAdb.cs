using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Community.Wsa.Sdk.Strategies.Packages;

namespace Community.Wsa.Sdk.Strategies.Api;

public interface IAdb
{
    bool IsInstalled { get; }

    string? PathToAdb { get; }

    Task InstallPackageAsync(
        string deviceSerialNumber,
        string filePath,
        bool allowDowngrade = false
    );

    Task UninstallPackageAsync(string deviceSerialNumber, string packageName);

    Task ConnectAsync(EndPoint endPoint);

    Task<KnownDevice[]> ListDevicesAsync();

    Task<PackageInfo[]> GetInstalledPackagesAsync(string deviceSerialNumber);

    Task<PackageInfo?> GetInstalledPackageAsync(string deviceSerialNumber, string packageName);

    Task LaunchPackageAsync(string deviceSerialNumber, string packageName);

    Task DisconnectAsync(EndPoint endPoint);
}
