using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Community.Wsa.Sdk.Exceptions;
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
    public async Task ExecuteAdbCommandAsync_ShouldFailBecauseAdbIsNotInstalled()
    {
        var pm = A.Fake<IProcessManager>();
        var env = A.Fake<IEnvironment>();
        var io = A.Fake<IIo>();

        A.CallTo(() => env.GetEnvironmentVariable(A<string>._)).Returns("");

        A.CallTo(() => io.DirectoryExists(A<string>._)).Returns(false);
        A.CallTo(() => io.FileExists(A<string>._)).Returns(false);

        var adb = new AdbClient(pm, env, io);

        adb.IsInstalled.Should().BeFalse();

        var call = async () => await adb.InstallPackageAsync("sn", "fp");

        (await call.Should().ThrowAsync<AdbException>()).Where(
            (ex) => ex.Error == AdbError.AdbIsNotInstalled
        );

        A.CallTo(() => pm.Start(A<ProcessStartInfo>._)).MustNotHaveHappened();
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

    [Test]
    public async Task InstallPackageAsync_ShouldInstallPackage()
    {
        await AssertExecutedWith(
            "Success",
            (adb) => adb.InstallPackageAsync("sn", "fp"),
            "-s",
            "sn",
            "install",
            "fp"
        );
    }

    [Test]
    public async Task InstallPackageAsync_ShouldInstallPackageWithDowngrade()
    {
        await AssertExecutedWith(
            "Success",
            (adb) => adb.InstallPackageAsync("sn", "fp", true),
            "-s",
            "sn",
            "install",
            "fp",
            "-d"
        );
    }

    [Test]
    public async Task InstallPackageAsync_ShouldTimeout()
    {
        var pm = A.Fake<IProcessManager>();
        var env = A.Fake<IEnvironment>();
        var io = A.Fake<IIo>();
        var process = ProcessTestHelpers.TimesOut();

        A.CallTo(() => pm.Start(A<ProcessStartInfo>._)).Returns(process);

        var adb = new AdbClient(pm, env, io);

        var fakePathToAdb = "Ü:/adb.exe";
        adb.PathToAdb = fakePathToAdb;

        var call = async () => await adb.InstallPackageAsync("sn", "fp");

        (await call.Should().ThrowAsync<AdbException>()).Where(
            (ex) => ex.Error == AdbError.CommandTimedOut
        );

        A.CallTo(() => pm.Start(A<ProcessStartInfo>._)).MustHaveHappened();
        A.CallTo(() => process.Kill()).MustHaveHappened();
    }

    [Test]
    public async Task InstallPackageAsync_ShouldFailWithNegativeExitCode()
    {
        var pm = A.Fake<IProcessManager>();
        var env = A.Fake<IEnvironment>();
        var io = A.Fake<IIo>();
        var process = ProcessTestHelpers.Exits(exitCode: -1);

        A.CallTo(() => pm.Start(A<ProcessStartInfo>._)).Returns(process);

        var adb = new AdbClient(pm, env, io);

        var fakePathToAdb = "Ü:/adb.exe";
        adb.PathToAdb = fakePathToAdb;

        var call = async () => await adb.InstallPackageAsync("sn", "fp");

        (await call.Should().ThrowAsync<AdbException>()).Where(
            (ex) => ex.Error == AdbError.CommandFailed
        );

        A.CallTo(() => pm.Start(A<ProcessStartInfo>._)).MustHaveHappened();
    }

    [Test]
    public async Task InstallPackageAsync_ShouldFailBecauseOfWrongOutput()
    {
        var pm = A.Fake<IProcessManager>();
        var env = A.Fake<IEnvironment>();
        var io = A.Fake<IIo>();
        var process = ProcessTestHelpers.Exits(output: "foobar");

        ProcessStartInfo? actualStartInfo = null;

        A.CallTo(() => pm.Start(A<ProcessStartInfo>._))
            .Invokes((args) => actualStartInfo = (ProcessStartInfo)args.Arguments[0]!)
            .Returns(process);

        var adb = new AdbClient(pm, env, io);

        var fakePathToAdb = "Ü:/adb.exe";
        adb.PathToAdb = fakePathToAdb;

        var call = async () => await adb.InstallPackageAsync("sn", "fp");

        (await call.Should().ThrowAsync<AdbException>()).Where(
            (ex) => ex.Error == AdbError.CommandFinishedWithInvalidOutput
        );

        A.CallTo(() => pm.Start(A<ProcessStartInfo>._)).MustHaveHappened();
    }

    [Test]
    public async Task UninstallPackageAsync_ShouldUninstallPackage()
    {
        await AssertExecutedWith(
            "Success",
            (adb) => adb.UninstallPackageAsync("sn", "pn"),
            "-s",
            "sn",
            "uninstall",
            "pn"
        );
    }

    private static async Task AssertExecutedWith(
        string output,
        Func<IAdb, Task> act,
        params string[] args
    )
    {
        var pm = A.Fake<IProcessManager>();
        var env = A.Fake<IEnvironment>();
        var io = A.Fake<IIo>();
        var process = ProcessTestHelpers.Exits(output: output, exitCode: 0);

        ProcessStartInfo? actualStartInfo = null;

        A.CallTo(() => pm.Start(A<ProcessStartInfo>._))
            .Invokes((args) => actualStartInfo = (ProcessStartInfo)args.Arguments[0]!)
            .Returns(process);

        var adb = new AdbClient(pm, env, io);

        var fakePathToAdb = "Ü:/adb.exe";
        adb.PathToAdb = fakePathToAdb;

        await act(adb);

        actualStartInfo.Should().NotBeNull();
        actualStartInfo!.FileName.Should().BeEquivalentTo(fakePathToAdb);
        actualStartInfo.ArgumentList.Should().Equal(args);

        A.CallTo(() => pm.Start(A<ProcessStartInfo>._)).MustHaveHappened();
    }

    [Test]
    public async Task ConnectAsync_ShouldConnect()
    {
        await AssertExecutedWith(
            "List of devices attached",
            (adb) => adb.ListDevicesAsync(),
            "devices",
            "-l"
        );
    }

    [Test]
    public async Task ListDevicesAsync_ShouldListDevicesEmptyList()
    {
        await AssertExecutedWith(
            "foobar",
            (adb) => adb.ConnectAsync(new DnsEndPoint("host", 1)),
            "connect",
            "host:1"
        );
    }

    [Test]
    public async Task ListDevicesAsync_ShouldListDevices()
    {
        var pm = A.Fake<IProcessManager>();
        var env = A.Fake<IEnvironment>();
        var io = A.Fake<IIo>();
        var process = ProcessTestHelpers.Exits(
            output: @"
List of devices attached
sn1 device   product:p1 model:m1 device:d1 transport_id:1
sn2 offline  product:p2 model:m2 device:d2 transport_id:2
sn3 emulator product:p3 model:m3 device:d3 transport_id:3
".Trim()
        );

        A.CallTo(() => pm.Start(A<ProcessStartInfo>._)).Returns(process);

        var adb = new AdbClient(pm, env, io);

        var fakePathToAdb = "Ü:/adb.exe";
        adb.PathToAdb = fakePathToAdb;

        var devices = await adb.ListDevicesAsync();

        A.CallTo(() => pm.Start(A<ProcessStartInfo>._)).MustHaveHappened();

        devices
            .Should()
            .Equal(
                new KnownDevice()
                {
                    DeviceSerialNumber = "sn1",
                    DeviceCode = "d1",
                    DeviceType = DeviceType.Device,
                    ModelNumber = "m1",
                    ProductCode = "p1",
                    TransportId = "1"
                },
                new KnownDevice()
                {
                    DeviceSerialNumber = "sn2",
                    DeviceCode = "d2",
                    DeviceType = DeviceType.Offline,
                    ModelNumber = "m2",
                    ProductCode = "p2",
                    TransportId = "2"
                },
                new KnownDevice()
                {
                    DeviceSerialNumber = "sn3",
                    DeviceCode = "d3",
                    DeviceType = DeviceType.Emulator,
                    ModelNumber = "m3",
                    ProductCode = "p3",
                    TransportId = "3"
                }
            );
    }

    [Test]
    public async Task ListDevicesAsync_ShouldListDevicesWithCustomEndPoint()
    {
        var ep = A.Fake<EndPoint>();

        A.CallTo(() => ep.ToString()).Returns("foobar");
        A.CallTo(() => ep.AddressFamily).Returns(AddressFamily.InterNetworkV6);

        await AssertExecutedWith("foobar", (adb) => adb.ConnectAsync(ep), "connect", "foobar");
    }
}
