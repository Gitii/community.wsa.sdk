using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Community.Wsa.Sdk.Exceptions;
using Community.Wsx.Shared;

namespace Community.Wsa.Sdk;

/// <inheritdoc />
public class AdbClient : IAdb
{
    private readonly IEnvironment _environment;
    private readonly IIo _io;
    private readonly IProcessManager _processManager;

    public AdbClient(
        IProcessManager? processManager = null,
        IEnvironment? environment = null,
        IIo? io = null
    )
    {
        _processManager = processManager ?? new Win32ProcessManager();
        _environment = environment ?? new Win32Environment();
        _io = io ?? new Win32IO();
    }

    /// <inheritdoc />
    public bool IsInstalled => CheckIfAdbIsAvailable();

    /// <inheritdoc />
    public string? PathToAdb { get; set; }

    /// <inheritdoc />
    public bool RestartServerOnCommandTimeout { get; set; } = true;

    /// <inheritdoc />
    public Task InstallPackageAsync(
        string deviceSerialNumber,
        string filePath,
        bool allowDowngrade = false
    )
    {
        return ExecuteAdbCommandAsync(
            new[] { "-s", deviceSerialNumber, "install", filePath, allowDowngrade ? "-d" : "" },
            outputMustInclude: "Success"
        );
    }

    /// <inheritdoc />
    public Task UninstallPackageAsync(string deviceSerialNumber, string packageName)
    {
        return ExecuteAdbCommandAsync(
            new[] { "-s", deviceSerialNumber, "uninstall", packageName },
            outputMustInclude: "Success"
        );
    }

    /// <inheritdoc />
    public Task ConnectAsync(EndPoint endPoint)
    {
        var address = GetAddress(endPoint);
        return ExecuteAdbCommandAsync(
            new[] { "connect", address },
            outputMustNotInclude: $"cannot connect to {address}"
        );
    }

    /// <inheritdoc />
    public async Task<KnownDevice[]> ListDevicesAsync()
    {
        var lines = await ExecuteAdbCommandAsync(
                new string[] { "devices", "-l" },
                outputMustInclude: "List of devices attached"
            )
            .ConfigureAwait(false);
        return lines
            .Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Skip(1)
            .Select(ParseLine)
            .ToArray();
    }

    /// <inheritdoc />
    public async Task<PackageInfo[]> GetInstalledPackagesAsync(string deviceSerialNumber)
    {
        var rawPackageNames = await ExecuteAdbCommandAsync(
                new string[] { "-s", deviceSerialNumber, "shell", "pm", "list", "packages", "-3" }
            )
            .ConfigureAwait(false);

        var packageNames = rawPackageNames.Split("\n");
        IList<PackageInfo> packages = new List<PackageInfo>(packageNames.Length);

        foreach (var packageName in packageNames)
        {
            const string PACKAGE_PREFIX = "package:";
            if (!packageName.StartsWith(PACKAGE_PREFIX, StringComparison.Ordinal))
            {
                continue;
            }

            packages.Add(
                await GetPackageDumpAsync(
                        deviceSerialNumber,
                        packageName.Substring(PACKAGE_PREFIX.Length).Trim()
                    )
                    .ConfigureAwait(false)
            );
        }

        return packages.ToArray();
    }

    /// <inheritdoc />
    public async Task<PackageInfo?> GetInstalledPackageAsync(
        string deviceSerialNumber,
        string packageName
    )
    {
        try
        {
            return await GetPackageDumpAsync(deviceSerialNumber, packageName).ConfigureAwait(false);
        }
        catch (AdbException e) when (e.Error == AdbError.CommandFinishedWithInvalidOutput)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public Task LaunchPackageAsync(string deviceSerialNumber, string packageName)
    {
        return ExecuteAdbCommandAsync(
            new[] { "-s", deviceSerialNumber, "shell", "monkey", "-p", packageName, "1" },
            outputMustInclude: "Events injected: 1"
        );
    }

    /// <inheritdoc />
    public Task DisconnectAsync(EndPoint endPoint)
    {
        var address = GetAddress(endPoint);
        return ExecuteAdbCommandAsync(
            new[] { "disconnect", address },
            outputMustInclude: $"disconnected {address}"
        );
    }

    /// <inheritdoc />
    public async Task<string> ExecuteCommandAsync(string command, params string[] arguments)
    {
        var adbArguments = new string[arguments.Length + 2];
        Array.Copy(arguments, 0, adbArguments, 2, arguments.Length);

        adbArguments[0] = "shell";
        adbArguments[1] = command;

        return (await ExecuteAdbCommandAsync(adbArguments).ConfigureAwait(false)).Trim();
    }

    private bool CheckIfAdbIsAvailable()
    {
        if (PathToAdb == null)
        {
            var paths = _environment
                .GetEnvironmentVariable("PATH")
                .Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            var bestCandidate = FindValidPlatformToolsFolder(paths);
            PathToAdb =
                bestCandidate == null ? String.Empty : _io.Combine(bestCandidate, "adb.exe");
        }

        return PathToAdb != String.Empty;
    }

    private string? FindValidPlatformToolsFolder(string[] folderCandidates)
    {
        return folderCandidates
            .Where(_io.DirectoryExists)
            .Where(HasFiles("adb.exe", "AdbWinApi.dll", "fastboot.exe"))
            .FirstOrDefault();
    }

    private Func<string, bool> HasFiles(params string[] fileNames)
    {
        return (string directory) =>
            fileNames.All((fileName) => _io.FileExists(_io.Combine(directory, fileName)));
    }

    private string GetAddress(EndPoint endPoint)
    {
        var str =
            endPoint.ToString() ?? throw new Exception("Failed to get endpoint representation.");

        var prefix = endPoint.AddressFamily.ToString();
        if (str.StartsWith(prefix, StringComparison.Ordinal))
        {
            return str.Substring(prefix.Length + 1);
        }

        return str;
    }

    private async Task<PackageInfo> GetPackageDumpAsync(
        string deviceSerialNumber,
        string packageName
    )
    {
        var dump = await ExecuteAdbCommandAsync(
                new string[]
                {
                    "-s",
                    deviceSerialNumber,
                    "shell",
                    "dumpsys",
                    "package",
                    packageName
                },
                outputMustNotInclude: $"Unable to find package: {packageName}"
            )
            .ConfigureAwait(false);

        return new PackageInfo()
        {
            PackageName = packageName,
            DisplayName = String.Empty,
            VersionCode = ExtractValue("versionCode="),
            DisplayVersion = ExtractValue("versionName="),
            DisplayIcon = Array.Empty<byte>(),
            Publisher = String.Empty,
            InstallDate = DateOnly.ParseExact(
                ExtractValue("firstInstallTime="),
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture
            ),
        };

        string ExtractValue(string key)
        {
            var index = dump.IndexOf(key, StringComparison.Ordinal);
            if (index < 0)
            {
                throw new Exception(
                    $"Failed to find key {key} in dump of {packageName} on device {deviceSerialNumber}"
                );
            }

            var valueStartIndex = index + key.Length;
            var valueEndIndex = dump.FindIndex(Char.IsWhiteSpace, valueStartIndex);

            if (valueEndIndex < 0)
            {
                throw new Exception(
                    $"Failed to end of value for key {key} in dump of {packageName} on device {deviceSerialNumber}"
                );
            }

            return dump.Substring(valueStartIndex, valueEndIndex - valueStartIndex);
        }
    }

    private KnownDevice ParseLine(string line)
    {
        var parts = line.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );
        if (parts.Length < 2)
        {
            throw new Exception("Invalid adb device line");
        }

        return new KnownDevice()
        {
            DeviceSerialNumber = parts[0],
            DeviceType = ParseDeviceType(parts[1]),
            ProductCode = FindProperty("product"),
            DeviceCode = FindProperty("device"),
            ModelNumber = FindProperty("model"),
            TransportId = FindProperty("transport_id")
        };

        string FindProperty(string prefix)
        {
            return parts
                    .FirstOrDefault(
                        (prop) => prop.StartsWith(prefix + ":", StringComparison.Ordinal)
                    )
                    ?.Remove(0, prefix.Length + 1) ?? string.Empty;
        }

        DeviceType ParseDeviceType(string rawDeviceType)
        {
            return rawDeviceType.ToLowerInvariant() switch
            {
                "offline" => DeviceType.Offline,
                "device" => DeviceType.Device,
                "emulator" => DeviceType.Emulator,
                "unauthorized" => DeviceType.Unauthorized,
                _
                  => throw new ArgumentOutOfRangeException(
                      rawDeviceType,
                      $"Device type '{rawDeviceType}' is unknown!"
                  ),
            };
        }
    }

    internal virtual async Task<string> ExecuteAdbCommandAsync(
        string[] arguments,
        string? outputMustInclude = null,
        string? outputMustNotInclude = null,
        bool? restartServerOnTimeout = null
    )
    {
        if (!IsInstalled)
        {
            throw new AdbException(AdbError.AdbIsNotInstalled);
        }

        var (startInfo, strCommand) = BuildStartInfo(arguments);

        var process =
            _processManager.Start(startInfo) ?? throw new AdbException(AdbError.CannotStartAdb);

        var stdOutTask = process.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
        var stdErrTask = process.StandardError.ReadToEndAsync().ConfigureAwait(false);

        var exitCode = await process.WaitForExitAsync(10_000).ConfigureAwait(false);

        string stdOut;
        string stdErr;

        if (!exitCode.HasValue)
        {
            process.Kill();
            if (restartServerOnTimeout.GetValueOrDefault(RestartServerOnCommandTimeout))
            {
                return await RestartAndExecuteAgainAsync(
                        arguments,
                        outputMustInclude,
                        outputMustNotInclude
                    )
                    .ConfigureAwait(false);
            }
            else
            {
                stdOut = await stdOutTask;
                stdErr = await stdErrTask;
                throw new AdbException(
                    AdbError.CommandTimedOut,
                    strCommand,
                    stdOut + "\n" + stdErr
                );
            }
        }

        stdOut = await stdOutTask;
        stdErr = await stdErrTask;
        var completeOutput = stdOut + "\n" + stdErr;

        if (exitCode != 0)
        {
            throw new AdbException(AdbError.CommandFailed, strCommand, completeOutput);
        }

        ValidateOutput(outputMustInclude, outputMustNotInclude, stdOut, strCommand, completeOutput);

        return stdOut;
    }

    private (ProcessStartInfo startInfo, string strCommand) BuildStartInfo(string[] arguments)
    {
        var startInfo = new ProcessStartInfo(PathToAdb!)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        var strCommand = $"adb {string.Join(" ", arguments)}";

        foreach (var argument in arguments.Where((s) => s.Length > 0))
        {
            startInfo.ArgumentList.Add(argument);
        }

        return (startInfo, strCommand);
    }

    private async Task<string> RestartAndExecuteAgainAsync(
        string[] arguments,
        string? outputMustInclude,
        string? outputMustNotInclude
    )
    {
        await ExecuteAdbCommandAsync(new string[] { "kill-server" }, restartServerOnTimeout: false)
            .ConfigureAwait(false);
        await ExecuteAdbCommandAsync(new string[] { "start-server" }, restartServerOnTimeout: false)
            .ConfigureAwait(false);

        return await ExecuteAdbCommandAsync(
                arguments,
                outputMustInclude,
                outputMustNotInclude,
                false
            )
            .ConfigureAwait(false);
    }

    private static void ValidateOutput(
        string? outputMustInclude,
        string? outputMustNotInclude,
        string stdOut,
        string strCommand,
        string completeOutput
    )
    {
        if (
            !string.IsNullOrEmpty(outputMustInclude)
            && !stdOut.Contains(outputMustInclude, StringComparison.OrdinalIgnoreCase)
        )
        {
            throw new AdbException(
                AdbError.CommandFinishedWithInvalidOutput,
                strCommand,
                completeOutput
            );
        }

        if (
            !string.IsNullOrEmpty(outputMustNotInclude)
            && stdOut.Contains(outputMustNotInclude, StringComparison.OrdinalIgnoreCase)
        )
        {
            throw new AdbException(
                AdbError.CommandFinishedWithInvalidOutput,
                strCommand,
                completeOutput
            );
        }
    }
}
