using Microsoft.Win32;

namespace Community.Wsx.Shared;

public interface IRegistryKey : IDisposable
{
    public T GetValue<T>(string name);
    public T GetValue<T>(string name, T defaultValue);
    public string[] GetSubKeyNames();

    /// <summary>
    /// <inheritdoc cref="RegistryKey.OpenSubKey(string)"/>
    /// </summary>
    public IRegistryKey OpenSubKey(string subKey);
}
