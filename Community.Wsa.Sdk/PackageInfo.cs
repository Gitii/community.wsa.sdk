using System;

namespace Community.Wsa.Sdk;

/// <summary>
/// Package information about a android application.
/// </summary>
public readonly struct PackageInfo
{
    /// <summary>
    /// Name of the package.
    /// </summary>
    public string PackageName { get; init; }

    /// <summary>
    /// The version code of the package.
    /// </summary>
    public string VersionCode { get; init; }

    /// <summary>
    /// The display name of the package.
    /// </summary>
    public string DisplayName { get; init; }

    /// <summary>
    /// Gets the icon of the package (encoded as png image).
    /// </summary>
    public byte[] DisplayIcon { get; init; }

    /// <summary>
    /// The display version of the package.
    /// </summary>
    public string DisplayVersion { get; init; }

    /// <summary>
    /// The install date of the package.
    /// </summary>
    public DateOnly InstallDate { get; init; }

    /// <summary>
    /// The publisher name of the package.
    /// </summary>
    public string Publisher { get; init; }

    /// <summary>
    /// List of capabilities of the package.
    /// </summary>
    public string[] Capabilities { get; init; }
}
