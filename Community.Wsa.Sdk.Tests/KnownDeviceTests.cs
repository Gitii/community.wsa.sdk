using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Community.Wsa.Sdk.Tests;

public class KnownDeviceTests
{
    [TestCase(DeviceType.Device, true, false, false)]
    [TestCase(DeviceType.Emulator, false, true, false)]
    [TestCase(DeviceType.Offline, false, false, true)]
    public void Constructor_ShouldHaveSameValuesAfterInit(
        DeviceType type,
        bool isDevice,
        bool isEmulator,
        bool isOffline
    )
    {
        var knd = new KnownDevice()
        {
            DeviceCode = "dc",
            DeviceSerialNumber = "sn",
            DeviceType = type,
            ModelNumber = "mn",
            ProductCode = "pc",
            TransportId = "ti",
        };

        knd.DeviceCode.Should().Be("dc");
        knd.DeviceSerialNumber.Should().Be("sn");
        knd.DeviceType.Should().Be(type);
        knd.ModelNumber.Should().Be("mn");
        knd.ProductCode.Should().Be("pc");
        knd.TransportId.Should().Be("ti");

        knd.IsDevice.Should().Be(isDevice);
        knd.IsEmulator.Should().Be(isEmulator);
        knd.IsOffline.Should().Be(isOffline);
    }
}
