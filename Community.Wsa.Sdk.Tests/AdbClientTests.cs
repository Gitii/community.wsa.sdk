using System.IO;
using Community.Wsx.Shared;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace Community.Wsa.Sdk.Tests;

public class AdbClientTests
{
    [Test]
    public void IsInstalled_PathToAdb_ShouldReturnTrueAndPathWhenToolsAreInstalled()
    {
        var pm = A.Fake<IProcessManager>();
        var env = A.Fake<IEnvironment>();
        var io = A.Fake<IIo>();

        A.CallTo(() => env.GetEnvironmentVariable(A<string>._)).Returns("C:\\;D:\\");

        A.CallTo(() => io.DirectoryExists("D:\\")).Returns(true);
        A.CallTo(() => io.FileExists(A<string>._)).Returns(true);

        A.CallTo(() => io.Combine(A<string[]>._))
            .ReturnsLazily((o) => Path.Combine(o.Arguments.Get<string[]>(0)!));

        var adb = new AdbClient(pm, env, io);

        adb.PathToAdb.Should().BeNull();
        adb.IsInstalled.Should().BeTrue();
        adb.PathToAdb.Should().Be("D:\\adb.exe");
    }

    [Test]
    public void IsInstalled_PathToAdb_ShouldReturnFalseAndEmptyPathWhenToolsAreNotInstalled()
    {
        var pm = A.Fake<IProcessManager>();
        var env = A.Fake<IEnvironment>();
        var io = A.Fake<IIo>();

        A.CallTo(() => env.GetEnvironmentVariable(A<string>._)).Returns("C:\\;D:\\");

        A.CallTo(() => io.DirectoryExists("D:\\")).Returns(true);
        A.CallTo(() => io.FileExists(A<string>._)).Returns(false);

        A.CallTo(() => io.Combine(A<string[]>._))
            .ReturnsLazily((o) => Path.Combine(o.Arguments.Get<string[]>(0)!));

        var adb = new AdbClient(pm, env, io);

        adb.PathToAdb.Should().BeNull();
        adb.IsInstalled.Should().BeFalse();
        adb.PathToAdb.Should().BeEmpty();
    }
}
