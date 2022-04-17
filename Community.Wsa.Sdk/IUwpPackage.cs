using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;

namespace Community.Wsa.Sdk;

/// <inheritdoc cref="Package"/>
public interface IUwpPackage
{
    /// <inheritdoc cref="PackageId.FamilyName"/>
    string FamilyName { get; }

    /// <inheritdoc cref="AppListEntry.LaunchAsync"/>
    Task LaunchAsync();
}
