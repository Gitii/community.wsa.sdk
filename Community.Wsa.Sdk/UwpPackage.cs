using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace Community.Wsa.Sdk;

internal class UwpPackage : IUwpPackage
{
    private readonly Package _package;

    public UwpPackage(Package package)
    {
        _package = package;
    }

    public string FamilyName =>
        _package.Id.ToString() ?? throw new Exception("Package doesn't have a family name!");

    public async Task Launch()
    {
        var entries = await _package.GetAppListEntriesAsync();
        var appEntry = entries.FirstOrDefault();

        if (appEntry != null)
        {
            await appEntry.LaunchAsync();
        }
    }
}
