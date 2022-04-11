using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Community.Wsx.Shared;

namespace Community.Wsa.Sdk;

public class WsaPackageManager : IPackageManager
{
    private readonly IAdb _adb;
    private readonly IRegistry _registry;
    private readonly IIo _io;
    private readonly IWsaClient _wsaClient;

    public WsaPackageManager(
        IEnvironment? environment = null,
        IProcessManager? processManager = null,
        IRegistry? registry = null,
        IIo? io = null,
        IWsaClient? wsaClient = null,
        IAdb? adb = null
    )
    {
        _registry = registry ?? new Win32Registry();
        _io = io ?? new Win32IO();
        _wsaClient = wsaClient ?? new ManagedWsaClient(environment, _io, processManager);
        _adb = adb ?? new AdbClient(processManager, environment, _io);
    }

    public Task InstallPackageAsync(
        string deviceId,
        string filePath,
        IProgress<string>? createStatusReporter = null
    )
    {
        return _adb.InstallPackageAsync(deviceId, filePath);
    }

    public async Task<PackageInfo[]> GetAllInstalledPackagesAsync(string deviceId)
    {
        List<PackageInfo> apps = new List<PackageInfo>();

        foreach (var appKey in GetAndroidAppKeys())
        {
            apps.Add(await GetPackageInfoFromKeyAsync(appKey).ConfigureAwait(false));

            appKey.Dispose();
        }

        return apps.ToArray();
    }

    private async Task<PackageInfo> GetPackageInfoFromKeyAsync(IRegistryKey appKey)
    {
        return new PackageInfo()
        {
            PackageName = appKey.GetValue<string>("AndroidPackageName"),
            DisplayName = appKey.GetValue<string>("DisplayName"),
            DisplayVersion = appKey.GetValue<string>("DisplayVersion"),
            VersionCode = appKey.GetValue<string>("AndroidVersionCode"),
            DisplayIcon = await GetPngFromFileAsync(appKey.GetValue<string>("DisplayIcon"))
                .ConfigureAwait(false),
            Publisher = appKey.GetValue<string>("Publisher"),
            InstallDate = DateOnly.ParseExact(
                appKey.GetValue<string>("InstallDate"),
                "yyyyMMdd",
                CultureInfo.InvariantCulture
            ),
            Capabilities = Array.Empty<string>(),
        };
    }

    private Task<byte[]> GetPngFromFileAsync(string filePath)
    {
        return Task.Run(
            async () =>
            {
                using var ico = new System.Drawing.Icon(filePath);
                await using var buffer = new MemoryStream();
                using var icoImage = ico.ToBitmap();

                icoImage.Save(buffer, ImageFormat.Png);

                return buffer.ToArray();
            }
        );
    }

    private IEnumerable<IRegistryKey> GetAndroidAppKeys()
    {
        using var installedApps = GetInstalledAppsKey();

        foreach (var appId in installedApps.GetSubKeyNames())
        {
            var app = installedApps.OpenSubKey(appId);

            if (app.GetSubKeyNames().Contains("AndroidPackageName"))
            {
                yield return app;
            }
        }
    }

    private IRegistryKey GetInstalledAppsKey()
    {
        using var hkcu = _registry.GetCurrentUser();
        var installedApps = hkcu.OpenSubKey(
            "Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall"
        );
        return installedApps;
    }

    public async Task<PackageInfo?> GetInstalledPackageAsync(string deviceId, string packageName)
    {
        using var appsKey = GetInstalledAppsKey();

        if (!appsKey.GetSubKeyNames().Contains(packageName))
        {
            return null;
        }

        using var appKey = appsKey.OpenSubKey(packageName);

        return await GetPackageInfoFromKeyAsync(appKey).ConfigureAwait(false);
    }

    public Task<bool> IsPackageInstalledAsync(string deviceId, string packageName)
    {
        using var appsKey = GetInstalledAppsKey();

        return Task.FromResult(appsKey.GetSubKeyNames().Contains(packageName));
    }

    public Task UninstallPackageAsync(string deviceId, string packageName)
    {
        return _wsaClient.UninstallAsync(packageName);
    }

    public Task LaunchAsync(string deviceId, string packageName)
    {
        return _wsaClient.LaunchAsync(packageName);
    }
}
