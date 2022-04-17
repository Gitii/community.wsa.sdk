using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Community.Wsa.Sdk.Tests;

public class PackageInfoTests
{
    [Test]
    public void Constructor_ShouldHaveSameValuesAfterInit()
    {
        var pi = new PackageInfo()
        {
            Publisher = "pub",
            VersionCode = "vc",
            DisplayVersion = "dv",
            PackageName = "pn",
            DisplayName = "dn",
            InstallDate = new DateOnly(2021, 05, 06),
            DisplayIcon = Array.Empty<byte>(),
            Capabilities = new[] { "a", "b" }
        };

        pi.Publisher.Should().Be("pub");
        pi.VersionCode.Should().Be("vc");
        pi.DisplayVersion.Should().Be("dv");
        pi.PackageName.Should().Be("pn");
        pi.DisplayName.Should().Be("dn");
        pi.InstallDate.Should().Be(new DateOnly(2021, 05, 06));
        pi.DisplayIcon.Should().BeEmpty();
        pi.Capabilities.Should().BeEquivalentTo("a", "b");
    }
}
