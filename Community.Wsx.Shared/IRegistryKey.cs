using Microsoft.Win32;

namespace Community.Wsx.Shared;

/// <inheritdoc cref="RegistryKey"/>
public interface IRegistryKey : IDisposable
{
    /// <inheritdoc cref="RegistryKey.GetValue(string?)"/>
    public T GetValue<T>(string name);

    /// <inheritdoc cref="RegistryKey.GetValue(string?,object?)"/>
    public T GetValue<T>(string name, T defaultValue);

    /// <inheritdoc cref="RegistryKey.GetSubKeyNames"/>
    public string[] GetSubKeyNames();

    /// <inheritdoc cref="RegistryKey.OpenSubKey(string)"/>
    public IRegistryKey OpenSubKey(string subKey);
}
