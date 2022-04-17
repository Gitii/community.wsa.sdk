using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Community.Wsa.Sdk.Exceptions;
using Community.Wsx.Shared;

namespace Community.Wsa.Sdk;

/// <inheritdoc />
public class WsaApi : IWsaApi
{
    private readonly IIo _io;
    private readonly IEnvironment _environment;
    private readonly IWsaClient _wsaClient;
    private readonly IAdb _adb;
    private readonly IProcessManager _processManager;

    public WsaApi(
        IAdb? adb = null,
        IWsaClient? wsaClient = null,
        IIo? io = null,
        IEnvironment? environment = null,
        IProcessManager? processManager = null
    )
    {
        _processManager = processManager ?? new Win32ProcessManager();
        _io = io ?? new Win32IO();
        _environment = environment ?? new Win32Environment();
        _wsaClient = wsaClient ?? new ManagedWsaClient(_environment, _io, processManager);
        _adb = adb ?? new AdbClient(processManager, environment, _io);

        IsWsaInstalled = _wsaClient.IsWsaInstalled;
    }

    /// <inheritdoc />
    public bool IsWsaSupported()
    {
        return IsWsaSupported(out _);
    }

    /// <inheritdoc />
    public bool IsWsaSupported(out string? missingCapabilities)
    {
        missingCapabilities = null;

        var commonErrorMessage =
            "Windows Subsystems for Android requires 64-bit system and Windows 11 or higher";

        if (!_environment.Is64BitOperatingSystem || !_environment.Is64BitProcess)
        {
            missingCapabilities = commonErrorMessage;
            return false;
        }

        if (_environment.OSVersion.Platform != PlatformID.Win32NT)
        {
            missingCapabilities = commonErrorMessage;
            return false;
        }

        if (
            _environment.OSVersion.Version.Major < 10
            || _environment.OSVersion.Version.Minor < 0
            || _environment.OSVersion.Version.Build < 22000
        )
        {
            missingCapabilities = commonErrorMessage;
            return false;
        }

        if (!IsWsaInstalled)
        {
            missingCapabilities = "This system does not have WSA installed.";
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    public bool IsRunning
    {
        get
        {
            Mutex? mutex;
            var mux = Mutex.TryOpenExisting(IWsaApi.WSA_MUTEX, out mutex);
            if (mux)
            {
                mutex?.Close();
                return true;
            }

            return false;
        }
    }

    /// <inheritdoc />
    public bool IsWsaInstalled { get; }

    /// <inheritdoc />
    public async Task EnsureWsaIsReadyAsync(IProgress<string>? progress = null)
    {
        if (!IsRunning)
        {
            progress?.Report("Starting Windows Subsystem for Android...");
            await StartServiceAsync().ConfigureAwait(false);
            await WaitForOpenPortAsync().ConfigureAwait(false);
        }

        progress?.Report("Connecting to Windows Subsystem for Android...");
        await EnsureWsaIsConnectedAsync(progress).ConfigureAwait(false);
        await _adb.ConnectAsync(new DnsEndPoint(IWsaApi.WSA_HOST_NAME, IWsaApi.WSA_PORT))
            .ConfigureAwait(false);
    }

    private async Task EnsureWsaIsConnectedAsync(IProgress<string>? progress = null)
    {
        bool connected = false;
        bool triedToConnect = false;
        var endPoint = new DnsEndPoint(
            IWsaApi.WSA_HOST_NAME,
            IWsaApi.WSA_PORT,
            AddressFamily.InterNetwork
        );

        do
        {
            progress?.Report("Querying connect devices...");
            var devices = await _adb.ListDevicesAsync().ConfigureAwait(false);
            var wsa = FindWsaDevice(devices);

            if (wsa.Equals(default(KnownDevice)))
            {
                if (triedToConnect)
                {
                    throw new ServiceException(ServiceError.CannotConnectToDevice);
                }

                triedToConnect = true;
                progress?.Report("Connecting to wsa device...");
                await _adb.ConnectAsync(endPoint).ConfigureAwait(false);
            }
            else if (wsa.IsOffline)
            {
                progress?.Report("Reconnecting to wsa device...");
                await _adb.DisconnectAsync(endPoint).ConfigureAwait(false);

                await _adb.ConnectAsync(endPoint).ConfigureAwait(false);
            }
            else
            {
                connected = wsa.IsDevice;
            }
        } while (!connected);

        progress?.Report("Connected to wsa device");
    }

    private KnownDevice FindWsaDevice(KnownDevice[] devices)
    {
        return devices.FirstOrDefault(
            (d) =>
                d.ModelNumber.Equals(IWsaApi.WSA_MODEL_NUMBER, StringComparison.OrdinalIgnoreCase)
        );
    }

    private async Task WaitForOpenPortAsync()
    {
        using TcpClient tcpClient = new TcpClient();

        bool portIsOpen = false;
        int tries = 60;
        do
        {
            try
            {
#pragma warning disable AsyncFixer02 // Long-running or blocking operations inside an async method
                tcpClient.Connect(IWsaApi.WSA_HOST_NAME, IWsaApi.WSA_PORT);
#pragma warning restore AsyncFixer02 // Long-running or blocking operations inside an async method
                portIsOpen = true;
            }
            catch (SocketException)
            {
                // port is not open, try again
                tries--;
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            }
        } while (!portIsOpen && tries > 0);

        if (!portIsOpen && tries <= 0)
        {
            throw new ServiceException(ServiceError.CannotConnectToService);
        }
    }

    /// <inheritdoc />
    public async Task StartServiceAsync()
    {
        try
        {
            ServiceController serviceController = new ServiceController("WsaService");
            if (serviceController.Status != ServiceControllerStatus.Running)
            {
                serviceController.Start();
                await WaitForServiceStatusAsync(serviceController, ServiceControllerStatus.Running)
                    .ConfigureAwait(false);
            }

            await EnsureThatWsaClientIsRunningAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            throw new ServiceException(ServiceError.CannotStartService, e);
        }
    }

    /// <inheritdoc />
    public async Task<string> GetDeviceIdAsync()
    {
        var devices = await _adb.ListDevicesAsync().ConfigureAwait(false);
        var wsa = FindWsaDevice(devices);

        if (wsa.Equals(default(KnownDevice)))
        {
            throw new ServiceException(ServiceError.CannotConnectToDevice);
        }

        return wsa.DeviceSerialNumber;
    }

    private Task EnsureThatWsaClientIsRunningAsync()
    {
        var p = _processManager.Find("WsaClient");
        if (p == null)
        {
            _processManager.Start(new ProcessStartInfo(_wsaClient.ProgramFilePath))?.Dispose();
        }

        p?.Dispose();

        return Task.CompletedTask;
    }

    private async Task WaitForServiceStatusAsync(
        ServiceController serviceController,
        ServiceControllerStatus targetStatus
    )
    {
        var startDate = DateTime.UtcNow;
        serviceController.Refresh();

        while (serviceController.Status != targetStatus)
        {
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            if (DateTime.UtcNow.Subtract(startDate).TotalSeconds > 60)
            {
                throw new Exception(
                    $"Service {serviceController.DisplayName} failed to transition to target status {targetStatus}"
                );
            }

            serviceController.Refresh();
        }
    }
}
