using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Options;
using TcpForwardingService.Configuration;
using Exception = System.Exception;

namespace TcpForwardingService;

public class TcpWritersPool : IDisposable
{
    private readonly ConcurrentDictionary<IPEndPoint, TcpClient?> _clients = new();
    private readonly ILogger<TcpWritersPool> _logger;
    private readonly DestinationsSettings _settings;
    private readonly ConcurrentDictionary<IPEndPoint, StreamWriter?> _writers = new();

    public TcpWritersPool(IOptions<DestinationsSettings> options, ILogger<TcpWritersPool> logger)
    {
        _logger = logger;
        _settings = options.Value;

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

    public void Dispose()
    {
        foreach (var item in _writers)
        {
            item.Value?.Dispose();
            _clients[item.Key]?.Dispose();
        }
    }

    public ReadOnlyDictionary<IPEndPoint, StreamWriter?> GetWriters()
    {
        return _writers.AsReadOnly();
    }

    public void AddWriter(IPEndPoint ipEndPoint)
    {
        StreamWriter? writer = null;
        TcpClient? client = null;
        
        try
        {
            client = new TcpClient();
            client.Connect(ipEndPoint);
            writer = new StreamWriter(client.GetStream());
        }
        catch (Exception e)
        {
            _logger.LogError("Unable to create writer for endpoint: '{Endpoint}', Error: '{Error}'",
                ipEndPoint.ToString(), e.Message);
        }

        _clients[ipEndPoint] = client;
        _writers[ipEndPoint] = writer;
    }
}