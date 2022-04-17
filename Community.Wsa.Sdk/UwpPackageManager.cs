using System;
using System.Linq;
using Windows.Management.Deployment;

namespace Community.Wsa.Sdk;

/// <inheritdoc />
internal class UwpPackageManager : IUwpPackageManager
{
    Windows.Management.Deployment.PackageManager packageManager =
        new Windows.Management.Deployment.PackageManager();

    /// <inheritdoc />
    public IUwpPackage? FindPackage(string packageName, string packagePublisher)
    {
        var packages = packageManager.FindPackagesForUser(
            String.Empty,
            packageName,
            packagePublisher
        );

        var package = packages.SingleOrDefault();

        return package == null ? null : new UwpPackage(package);
    }
}
