using System;
using System.IO;
using System.Threading.Tasks;

namespace Community.Wsa.Sdk;

/// <summary>
/// Wrapper for the wsaclient executable.
/// </summary>
public interface IWsaClient
{
    /// <summary>
    /// File path to the wsaclient executable.
    /// </summary>
    /// <exception cref="Exception">Windows Subsystem for Android is not installed</exception>
    /// <exception cref="FileNotFoundException">Windows Subsystem for Android is installed but WsaClient.exe doesn't exist.</exception>
    string ProgramFilePath { get; }

    /// <summary>
    /// Verifies if wsa is installed.
    /// </summary>
    bool IsWsaInstalled { get; }

    /// <summary>
    /// Launches the specified package.
    /// </summary>
    /// <param name="packageName">Name of the package which will be started.</param>
    Task LaunchAsync(string packageName);

    /// <summary>
    /// Launches the wsa settings app.
    /// </summary>
    Task LaunchWsaSettingsAsync();

    /// <summary>
    /// Uninstalls the specified package.
    /// </summary>
    /// <param name="packageName">The name of the package which will be uninstalled.</param>
    Task UninstallAsync(string packageName);

    /// <summary>
    /// Launches an app associated by the passed in url/link. 
    /// </summary>
    /// <param name="link">The url/link which will be used.</param>
    Task LaunchDeepLinkAsync(string link);
}
