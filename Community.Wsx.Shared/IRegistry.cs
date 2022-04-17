using Microsoft.Win32;

namespace Community.Wsx.Shared;

/// <inheritdoc cref="Registry"/>
public interface IRegistry
{
    /// <inheritdoc cref="Registry.CurrentUser"/>
    IRegistryKey GetCurrentUser();
}
