using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Options;
using TcpForwardingService.Configuration;

namespace TcpForwardingService;

public class TcpWritersPool: IDisposable
{
    private readonly ILogger<TcpWritersPool> _logger;
    private readonly DestinationsSettings _settings;
    private Dictionary<IPEndPoint, StreamWriter?> _writers;

    public TcpWritersPool(IOptions<DestinationsSettings> options, ILogger<TcpWritersPool> logger)
    {
        _logger = logger;
        _settings = options.Value;

        // https://chat.openai.com/c/06f95145-6b2c-4ff1-9532-4fe8dfbd3d22

        foreach (var hostPort in _settings.Destinations)
            if (IPAddress.TryParse(hostPort.Host, out var ipAddress))
            {
                var ipEndPoint = new IPEndPoint(ipAddress, hostPort.Port);
                AddWriter(ipEndPoint);
            }
            else
            {
                _logger.LogError("Unable to instantiate IPEndPoint for the host '{Host}'", hostPort.Host);
            }
    }

    public ReadOnlyDictionary<IPEndPoint, StreamWriter?> GetWriters()
    {
        return _writers.AsReadOnly();
    }

    public void AddWriter(IPEndPoint ipEndPoint)
    {
        try
        {
            var client = new TcpClient();
            client.Connect(ipEndPoint);
            _writers.Add(ipEndPoint, new StreamWriter(client.GetStream()));
        }
        catch (Exception e)
        {
            _logger.LogError("Unable to create writer for destination: '{Endpoint}', Error: '{Error}'",
                ipEndPoint.ToString(), e.Message);
            _writers.Add(ipEndPoint, null);
        }
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
}