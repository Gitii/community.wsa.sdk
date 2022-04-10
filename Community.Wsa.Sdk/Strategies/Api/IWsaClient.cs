using System.Threading.Tasks;

namespace Community.Wsa.Sdk.Strategies.Api;

public interface IWsaClient
{
    string ProgramFilePath { get; }

    bool IsWsaInstalled { get; }

    Task LaunchAsync(string packageName);

    Task LaunchWsaSettingsAsync();

    Task UninstallAsync(string packageName);

    Task LaunchDeepLinkAsync(string link);
}
