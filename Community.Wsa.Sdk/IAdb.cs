using System.Net;
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
    /// If <c>true</c>, the adb server will be restarted (once) and the command then executed again.
    /// If <c>true</c> and the commands times out twice, the timeout exception will not be handled.
    /// If <c>false</c>, a timeout of command execution will not be handled.
    /// </summary>
    bool RestartServerOnCommandTimeout { get; set; }

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
    /// Returns detailed information about an installed package or <c>null</c> if it's not installed.
    /// </summary>
    /// <param name="deviceSerialNumber">The serial number of the device which will be targeted.</param>
    /// <param name="packageName">The name of the package which details will be returned.</param>
    Task<PackageInfo?> GetInstalledPackageAsync(string deviceSerialNumber, string packageName);

    /// <summary>
    /// Launches the specified app.
    /// </summary>
    /// <param name="deviceSerialNumber">The serial number of the device which will be targeted.</param>
    /// <param name="packageName">The name of the package which will be launched.</param>
    Task LaunchPackageAsync(string deviceSerialNumber, string packageName);

    /// <summary>
    /// Disconnects the specified endpoint.
    /// </summary>
    /// <param name="endPoint">The endpoint which will be disconnected.</param>
    Task DisconnectAsync(EndPoint endPoint);

    /// <summary>
    /// Executes the <paramref name="command"/> with zero or more <paramref name="arguments"/>.
    /// The output on standard output of the command will be returned.
    /// When the exit code of the executed command is negative, an exception will be thrown.
    /// </summary>
    /// <param name="command">The command or name of the executable to execute.</param>
    /// <param name="arguments">The list of arguments.</param>
    /// <returns>The output of the executed command.</returns>
    Task<string> ExecuteCommandAsync(string command, string[] arguments);
}
