using Microsoft.Win32;

namespace Community.Wsx.Shared;

internal class Win32Registry : IRegistry
{
    public IRegistryKey GetCurrentUser()
    {
        return new Win32RegistryKey(Registry.CurrentUser);
    }
}
