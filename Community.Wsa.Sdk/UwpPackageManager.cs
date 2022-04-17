using System;
using System.Linq;

namespace Community.Wsa.Sdk;

internal class UwpPackageManager : IUwpPackageManager
{
    Windows.Management.Deployment.PackageManager packageManager =
        new Windows.Management.Deployment.PackageManager();

    public IUwpPackage? FindPackage(string packageName, string packagePublisher)
    {
        var packages = packageManager.FindPackagesForUser(
            String.Empty,
            packageName,
            packagePublisher
        );

        var package = packages.SingleOrDefault();
        if (package == null)
        {
            return null;
        }

        return new UwpPackage(package);
    }
}
