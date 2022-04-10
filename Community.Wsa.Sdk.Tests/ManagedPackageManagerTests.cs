using Community.Wsa.Sdk.Strategies.Api;
using FluentAssertions;
using NUnit.Framework;

namespace Community.Wsa.Sdk.Tests;

public class ManagedPackageManagerTests
{
    [Test]
    public void Test1()
    {
        IWsaApi api = new Wsa.Sdk.Strategies.Api.WsaApi();

        var r = api.IsRunning;

        r.Should().BeTrue();
    }
}
