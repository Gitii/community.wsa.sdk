using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Community.Wsx.Shared;

namespace Community.Wsa.Sdk;

/// <inheritdoc />
public class ManagedWsaClient : IWsaClient
{
    private readonly IEnvironment _environment;
    private readonly IIo _io;
    private readonly IProcessManager _processManager;

    public ManagedWsaClient(
        IEnvironment? environment = null,
        IIo? io = null,
        IProcessManager? processManager = null
    )
    {
        _environment = environment ?? new Win32Environment();
        _io = io ?? new Win32IO();
        _processManager = processManager ?? new Win32ProcessManager();
    }

    private string _programFilePath = String.Empty;

    private static Package? GetWsaPackage()
    {
        Windows.Management.Deployment.PackageManager packageManager =
            new Windows.Management.Deployment.PackageManager();

        var packages = packageManager.FindPackagesForUser(
            String.Empty,
            "MicrosoftCorporationII.WindowsSubsystemForAndroid",
            "CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US"
        );

        return packages.SingleOrDefault();
    }

    /// <inheritdoc />
    public string ProgramFilePath
    {
        get
        {
            if (string.IsNullOrEmpty(_programFilePath))
            {
                var wsaPackage = GetWsaPackage();
                if (wsaPackage == null)
                {
                    throw new Exception("Windows Subsystem for Android is not installed");
                }

                var localAppDataDirectory = _environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData
                );

                _programFilePath = _io.Combine(
                    localAppDataDirectory,
                    "Microsoft",
                    "WindowsApps",
                    wsaPackage.Id.FamilyName,
                    "WsaClient.exe"
                );

                if (!File.Exists(_programFilePath))
                {
                    throw new FileNotFoundException(
                        "Windows Subsystem for Android is installed but WsaClient.exe doesn't exist.",
                        _programFilePath
                    );
                }
            }

            return _programFilePath;
        }
    }

    /// <inheritdoc />
    public bool IsWsaInstalled
    {
        get { return GetWsaPackage() != null; }
    }

    /// <inheritdoc />
    public Task LaunchAsync(string packageName)
    {
        return ExecuteAsync("launch", $"wsa://{packageName}");
    }

    /// <inheritdoc />
    public async Task LaunchWsaSettingsAsync()
    {
        var entries = await GetWsaPackage()?.GetAppListEntriesAsync();
        var appEntry = entries.FirstOrDefault();

        if (appEntry != null)
        {
            await appEntry.LaunchAsync();
        }
    }

    /// <inheritdoc />
    public Task UninstallAsync(string packageName)
    {
        return ExecuteAsync("/uninstall", packageName);
    }

    /// <inheritdoc />
    public Task LaunchDeepLinkAsync(string link)
    {
        return ExecuteAsync("/deeplink", link);
    }

    private async Task ExecuteAsync(params string[] args)
    {
        var startInfo = new ProcessStartInfo { CreateNoWindow = false, FileName = "cmd.exe" };

        startInfo.ArgumentList.Add("/K");
        startInfo.ArgumentList.Add(
            string.Join(" ", (new string[] { ProgramFilePath }).Concat(args))
        );

        var process = _processManager.Start(startInfo);

        if (process == null)
        {
            throw new Exception("Failed to start wsaclient");
        }

        var exitCode = await process.WaitForExitAsync().ConfigureAwait(false);

        if (exitCode != 0)
        {
            throw new Exception(
                $"wsaclient failed to execute command '{string.Join(" ", args)}' (Exit code is {exitCode})."
            );
        }
    }
}
