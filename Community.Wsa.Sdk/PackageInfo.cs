using System;

namespace Community.Wsa.Sdk;

public readonly struct PackageInfo
{
    public string PackageName { get; init; }
    public string VersionCode { get; init; }
    public string DisplayName { get; init; }

    /// <summary>
    /// Gets the icon of the package (encoded as png image).
    /// </summary>
    public byte[] DisplayIcon { get; init; }

    public string DisplayVersion { get; init; }
    public DateOnly InstallDate { get; init; }
    public string Publisher { get; init; }
    public string[] Capabilities { get; init; }
}
