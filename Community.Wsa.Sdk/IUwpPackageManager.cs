using System.Collections.Generic;
using System.Text;
using Windows.Management.Deployment;

namespace Community.Wsa.Sdk;

/// <inheritdoc cref="Windows.Management.Deployment.PackageManager"/>
public interface IUwpPackageManager
{
    /// <inheritdoc cref="PackageManager.FindPackageForUser"/>
    IUwpPackage? FindPackage(string packageName, string packagePublisher);
}
