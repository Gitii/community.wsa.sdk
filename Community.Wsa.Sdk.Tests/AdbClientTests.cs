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

    private static async Task<T> AssertExecutedWith<T>(
        string output,
        Func<IAdb, Task<T>> act,
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

        var result = await act(adb);

        actualStartInfo.Should().NotBeNull();
        actualStartInfo!.FileName.Should().BeEquivalentTo(fakePathToAdb);
        actualStartInfo.ArgumentList.Should().Equal(args);

        A.CallTo(() => pm.Start(A<ProcessStartInfo>._)).MustHaveHappened();

        return result;
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

    private static async Task AssertExecutionFailsWith<T>(
        string output,
        Func<IAdb, Task> act,
        string[] args,
        string errorMessagePattern
    ) where T : Exception
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

        var call = async () => await act(adb);

        (await call.Should().ThrowAsync<T>()).WithMessage(errorMessagePattern);

        actualStartInfo.Should().NotBeNull();
        actualStartInfo!.FileName.Should().BeEquivalentTo(fakePathToAdb);
        actualStartInfo.ArgumentList.Should().Equal(args);

        A.CallTo(() => pm.Start(A<ProcessStartInfo>._)).MustHaveHappened();
    }

    [Test]
    public async Task ConnectAsync_ShouldConnect()
    {
        await AssertExecutedWith(
            "",
            (adb) => adb.ConnectAsync(new DnsEndPoint("host", 1)),
            "connect",
            "host:1"
        );
    }

    [Test]
    public async Task ConnectAsync_ShouldFailBecauseCannotConnect()
    {
        await AssertExecutionFailsWith<AdbException>(
            "cannot connect to host:1",
            (adb) => adb.ConnectAsync(new DnsEndPoint("host", 1)),
            new string[] { "connect", "host:1" },
            "Adb command could be started and has finished but the output is invalid.\nCommand: adb connect host:1\ncannot connect to host:1"
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
sn3 emulator
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
                    DeviceCode = "",
                    DeviceType = DeviceType.Emulator,
                    ModelNumber = "",
                    ProductCode = "",
                    TransportId = ""
                }
            );
    }

    [TestCase("foobar", "Invalid adb device line")]
    [TestCase("sn1 unknown", "Device type 'unknown' is unknown! (Parameter 'unknown')")]
    public async Task ListDevicesAsync_ShouldFailBecauseDevicesOutputIsInvalid(
        string output,
        string expectedErrorMessage
    )
    {
        var pm = A.Fake<IProcessManager>();
        var env = A.Fake<IEnvironment>();
        var io = A.Fake<IIo>();
        var process = ProcessTestHelpers.Exits(
            output: $"List of devices attached{Environment.NewLine}{output}"
        );

        A.CallTo(() => pm.Start(A<ProcessStartInfo>._)).Returns(process);

        var adb = new AdbClient(pm, env, io);

        var fakePathToAdb = "Ü:/adb.exe";
        adb.PathToAdb = fakePathToAdb;

        var call = async () => await adb.ListDevicesAsync();

        (await call.Should().ThrowAsync<Exception>()).WithMessage(expectedErrorMessage);
    }

    [Test]
    public async Task ListDevicesAsync_ShouldListDevicesWithCustomEndPoint()
    {
        var ep = A.Fake<EndPoint>();

        A.CallTo(() => ep.ToString()).Returns("foobar");
        A.CallTo(() => ep.AddressFamily).Returns(AddressFamily.InterNetworkV6);

        await AssertExecutedWith("foobar", (adb) => adb.ConnectAsync(ep), "connect", "foobar");
    }

    [Test]
    public async Task GetInstalledPackagesAsync_ShouldListAllPackageInfos()
    {
        var pm = A.Fake<IProcessManager>();
        var env = A.Fake<IEnvironment>();
        var io = A.Fake<IIo>();
        var process = ProcessTestHelpers.Exits(
            output: @"
junk-heading
package:app-a
junk-1
package:app-b
junk-2
".Trim()
        );

        var appADumpProcess = ProcessTestHelpers.Exits(
            output: @"
versionCode=1   
versionName=a  
firstInstallTime=2022-01-01
"
        );
        var appBDumpProcess = ProcessTestHelpers.Exits(
            output: @"
versionCode=2
versionName=b
firstInstallTime=2022-01-01
"
        );

        A.CallTo(() => pm.Start(A<ProcessStartInfo>._))
            .Returns(process)
            .Once()
            .Then.Returns(appADumpProcess)
            .Once()
            .Then.Returns(appBDumpProcess);

        var adb = new AdbClient(pm, env, io);

        var fakePathToAdb = "Ü:/adb.exe";
        adb.PathToAdb = fakePathToAdb;

        var packages = await adb.GetInstalledPackagesAsync("sn1");

        A.CallTo(() => pm.Start(A<ProcessStartInfo>._)).MustHaveHappened(3, Times.Exactly);

        packages
            .Should()
            .Equal(
                new PackageInfo()
                {
                    PackageName = "app-a",
                    DisplayName = string.Empty,
                    VersionCode = "1",
                    DisplayVersion = "a",
                    DisplayIcon = Array.Empty<byte>(),
                    Publisher = string.Empty,
                    InstallDate = new DateOnly(2022, 1, 1),
                },
                new PackageInfo()
                {
                    PackageName = "app-b",
                    DisplayName = string.Empty,
                    VersionCode = "2",
                    DisplayVersion = "b",
                    DisplayIcon = Array.Empty<byte>(),
                    Publisher = string.Empty,
                    InstallDate = new DateOnly(2022, 1, 1),
                }
            );
    }

    [Test]
    public async Task GetInstalledPackageAsync_ShouldGetPackageInfo()
    {
        var pm = A.Fake<IProcessManager>();
        var env = A.Fake<IEnvironment>();
        var io = A.Fake<IIo>();

        var appADumpProcess = ProcessTestHelpers.Exits(
            output: @"
versionCode=1   
versionName=a  
firstInstallTime=2022-01-01
"
        );

        A.CallTo(() => pm.Start(A<ProcessStartInfo>._)).Returns(appADumpProcess);

        var adb = new AdbClient(pm, env, io);

        var fakePathToAdb = "Ü:/adb.exe";
        adb.PathToAdb = fakePathToAdb;

        var package = await adb.GetInstalledPackageAsync("sn1", "app-a");

        A.CallTo(() => pm.Start(A<ProcessStartInfo>._)).MustHaveHappened();

        package
            .Should()
            .BeEquivalentTo(
                new PackageInfo()
                {
                    PackageName = "app-a",
                    DisplayName = string.Empty,
                    VersionCode = "1",
                    DisplayVersion = "a",
                    DisplayIcon = Array.Empty<byte>(),
                    Publisher = string.Empty,
                    InstallDate = new DateOnly(2022, 1, 1),
                }
            );
    }

    [TestCase("foobar", "Failed to find key versionCode= in dump of app-a on device sn1")]
    [TestCase(
        "versionCode=a",
        "Failed to end of value for key versionCode= in dump of app-a on device sn1"
    )]
    public async Task GetInstalledPackageAsync_ShouldFailBecauseOutputFailed(
        string output,
        string expectedErrorMessage
    )
    {
        var pm = A.Fake<IProcessManager>();
        var env = A.Fake<IEnvironment>();
        var io = A.Fake<IIo>();

        var appADumpProcess = ProcessTestHelpers.Exits(output: output);

        A.CallTo(() => pm.Start(A<ProcessStartInfo>._)).Returns(appADumpProcess);

        var adb = new AdbClient(pm, env, io);

        var fakePathToAdb = "Ü:/adb.exe";
        adb.PathToAdb = fakePathToAdb;

        var call = async () => await adb.GetInstalledPackageAsync("sn1", "app-a");

        (await call.Should().ThrowAsync<Exception>()).WithMessage(expectedErrorMessage);

        A.CallTo(() => pm.Start(A<ProcessStartInfo>._)).MustHaveHappened();
    }

    [Test]
    public async Task GetInstalledPackageAsync_ShouldFailBecausePackageIsMissing()
    {
        var pm = A.Fake<IProcessManager>();
        var env = A.Fake<IEnvironment>();
        var io = A.Fake<IIo>();

        var appADumpProcess = ProcessTestHelpers.Exits(output: "Unable to find package: app-a");

        A.CallTo(() => pm.Start(A<ProcessStartInfo>._)).Returns(appADumpProcess);

        var adb = new AdbClient(pm, env, io);

        var fakePathToAdb = "Ü:/adb.exe";
        adb.PathToAdb = fakePathToAdb;

        var result = await adb.GetInstalledPackageAsync("sn1", "app-a");

        result.Should().BeNull();

        A.CallTo(() => pm.Start(A<ProcessStartInfo>._)).MustHaveHappened();
    }

    [Test]
    public async Task DisconnectAsync_ShouldFailBecauseCannotDisconnected()
    {
        await AssertExecutionFailsWith<AdbException>(
            "foobar",
            (adb) => adb.DisconnectAsync(new DnsEndPoint("host", 1)),
            new string[] { "disconnect", "host:1" },
            "Adb command could be started and has finished but the output is invalid.\nCommand: adb disconnect host:1\nfoobar"
        );
    }

    [Test]
    public async Task DisconnectAsync_ShouldDisconnect()
    {
        await AssertExecutedWith(
            "disconnected host:1",
            (adb) => adb.DisconnectAsync(new DnsEndPoint("host", 1)),
            "disconnect",
            "host:1"
        );
    }

    [Test]
    public async Task LaunchPackageAsync_ShouldFailBecauseCannotLaunch()
    {
        await AssertExecutionFailsWith<AdbException>(
            "foobar",
            (adb) => adb.LaunchPackageAsync("sn1", "app-a"),
            new string[] { "-s", "sn1", "shell", "monkey", "-p", "app-a", "1" },
            "Adb command could be started and has finished but the output is invalid.\nCommand: adb -s sn1 shell monkey -p app-a 1\nfoobar"
        );
    }

    [Test]
    public async Task LaunchPackageAsync_ShouldLaunch()
    {
        await AssertExecutedWith(
            "Events injected: 1",
            (adb) => adb.LaunchPackageAsync("sn1", "app-a"),
            "-s",
            "sn1",
            "shell",
            "monkey",
            "-p",
            "app-a",
            "1"
        );
    }

    [Test]
    public async Task ExecuteCommandAsync_ShouldExecute()
    {
        var actualOutput = await AssertExecutedWith(
            "foobar",
            (adb) => adb.ExecuteCommandAsync("test", new string[] { "a", "b" }),
            "shell",
            "test",
            "a",
            "b"
        );

        actualOutput.Should().BeEquivalentTo("foobar");
    }
}
