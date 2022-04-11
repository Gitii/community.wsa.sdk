namespace Community.Wsx.Shared;

internal class Win32Environment : IEnvironment
{
    public bool Is64BitOperatingSystem => Environment.Is64BitOperatingSystem;

    public bool Is64BitProcess => Environment.Is64BitProcess;

    public OperatingSystem OSVersion => Environment.OSVersion;

    public string GetFolderPath(Environment.SpecialFolder folder)
    {
        return Environment.GetFolderPath(folder);
    }

    public string GetEnvironmentVariable(string environmentVariableName)
    {
        return Environment.GetEnvironmentVariable(environmentVariableName) ?? String.Empty;
    }
}
