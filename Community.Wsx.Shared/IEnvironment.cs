namespace Community.Wsx.Shared;

public interface IEnvironment
{
    /// <inheritdoc cref="Environment.Is64BitOperatingSystem"/>
    public bool Is64BitOperatingSystem { get; }

    /// <inheritdoc cref="Environment.Is64BitProcess"/>
    public bool Is64BitProcess { get; }

    /// <inheritdoc cref="Environment.OSVersion"/>
    public System.OperatingSystem OSVersion { get; }

    /// <inheritdoc cref="Environment.GetFolderPath(System.Environment.SpecialFolder)"/>
    public string GetFolderPath(System.Environment.SpecialFolder folder);

    /// <inheritdoc cref="Environment.GetEnvironmentVariable(string)"/>
    public string GetEnvironmentVariable(string environmentVariableName);
}
