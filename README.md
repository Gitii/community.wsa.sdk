# Community SDK for Windows Subsystem for Android

This project contains a WSA API for Windows developers who wants to integrate WSA 
features into existing Windows applications. You can enumerate and query packages, install and uninstall packages via C# classes.

Physical devices and other emulators are also supported (see section [WSA and other android devices (for example pysical devices)](#WSA and other android devices (for example pysical devices))).

## Supported frameworks

- .Net 5 (`net5.0-windows10.0.19041.0` or higher)
- .Net 6 (`net6.0-windows10.0.19041.0` or higher)

## Supported Operating Systems

- Windows 10 19041 or higher
- Windows 11

## How to use

There are one default implementations of `IWsaApi`.

It uses the `wsaclient.exe` and `adb.exe` executable and (mostly) public information stored in the registry.

> :exclamation: [`platform-tools`](https://developer.android.com/studio/releases/platform-tools) need to be installed. They include the `adb.exe`. The folder containing `adb.exe` must be added to `%PATH%`.

## How to install

This package is available on [nuget.org](https://www.nuget.org/packages/Community.Wsa.Sdk).  
You can add a reference using `dotnet`:

```shell
dotnet add package Community.Wsa.Sdk
```

## API

### WSA Api

| Class               | Description                                                                                                                                                           |
| ------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `WsaApi`            | Check if `WSA` is installed or running. Allows you to start `WSA` if needed.                                                                                          |
| `WsaPackageManager` | List all installed packages, install new packages, upgrade, downgrade or uninstall existing packages. Uses `wsaclient.exe` (and `adb.exe` for packages installation). |
| `AdbPackageManager` | List all installed packages, install new packages, upgrade, downgrade or uninstall existing packages. Uses `adb.exe` for all operations.                              |

> :exclamation: `wsaclient.exe` sometimes locks up and never finishes. It is recommended to use `WsaPackageManager` for **readonly** operations (like querying of packages) and `AdbPackageManager` for `write` operations like installing and uninstalling packages. Both implement the same interface (`IPackageManager`).

## Code Examples

### Basic usage of wsa api

```csharp
using Community.Wsa.Sdk.Strategies.Api;

// Create instance
var api = new WsaApi();

// Check if wsa is supported
bool isSupported = api.IsWsaSupported();

// OR check if wsa is supported and also know why not:
string reason;
bool isSupported = api.IsWsaSupported(out reason);

// Check if wsa is installed
bool isSupported = api.IsWsaInstalled();

// Check if wsa is running
bool isRunning = api.IsRunning();

// Ensures that WSA is ready (started and adb is connected).
// NOTE: will fail if developer mode is not activated.
await api.EnsureWsaIsReadyAsync();

// Starts the WSA service if it's not already running.
await api.StartServiceAsync();
```

### Basic commands

```csharp
using Community.Wsa.Sdk.Strategies.Api;

// Setup
var api = new WsaApi();
var pkgm = new WsaPackageManager();

// Get the device serial number of WSA
// Usually this is 127.0.01:58526 or localhost:58526
// but that depends how (and who) connected adb to the WSA device.
// GetDeviceIdAsync will find it regardless.
var deviceId = await api.GetDeviceIdAsync();

// assume that we have a valid path to an existing apk file
// for example C:\app.apk
var apkPath = "..."; 

// Install package
await pkgm.InstallPackageAsync(deviceId, apkPath);

// Uninstall package 
await pkgm.UninstallPackageAsync(deviceId, "package.name.of.the.app");
```

### Available methods for `IPackageManager`

```csharp
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
    public Task InstallPackageAsync(string deviceId, string filePath, IProgress<string>? progress = null);

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
```

# Unit tests and Mocks

All classes in `Community.Wsa.Sdk` implement interfaces, for example `WsaApi` implements `IWsaApi`. If you already use mocking frameworks & DI, use them to create a *test friendly* instance, for example using `FakeItEasy`:

```
var api = A.Fake<IWsaApi>();
```

You can also mock specific parts of the implementation by passing custom implementations in the constructor:

```csharp
/*
Signature of the constructor:
public WsaApi(
    IAdb? adb = null,
    IWsaClient? wsaClient = null,
    IIo? io = null,
    IEnvironment? environment = null,
    IProcessManager? processManager = null
)
*/

// mock only the IIo logic
var api = new WsaApi(
    io: A.Fake<IIo>()
);
```

# WSA and `Developer Mode`

There is no known way to programmatically enable `Developer Mode` for `WSA`. It needs to be done manually, by the user.

`Community.Wsa.Sdk` requires `Developer Mode` to be enabled for all operations that rely on `adb` (all package related operations except querying and listing packages when `WsaPackageManager` is used).

# WSA and other android devices (for example pysical devices)

The WSA device is regular android system (running in an emulator). `Community.Wsa.Sdk` will work with any android system (physical or emulated), as long as **developer mode** is enabled.

Use `AdbClient` (for example `ListDevicesAsync` for listing all known devices) and `AdbPackageManager` (or `AdbClient`) for managing the packages.
