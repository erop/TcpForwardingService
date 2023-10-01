using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Options;
using TcpForwardingService.Configuration;

namespace TcpForwardingService;

public class Worker : BackgroundService
{
    private readonly DestinationsSettings _destinationsSettings;
    private readonly TcpListener _listener;
    private readonly ILogger<Worker> _logger;
    private readonly SourceSettings _sourceSettings;

    public Worker(ILogger<Worker> logger, IOptions<SourceSettings> sourceSettings,
        IOptions<DestinationsSettings> destinationsSettings)
    {
        _logger = logger;
        _sourceSettings = sourceSettings.Value;
        _destinationsSettings = destinationsSettings.Value;
        _listener = new TcpListener(new IPEndPoint(IPAddress.Parse(_sourceSettings.LocalIp), _sourceSettings.Port));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
            try
            {
                _listener.Start();
                _logger.LogInformation("Start listening on endpoint: {Endpoint}", _listener.LocalEndpoint.ToString());

                while (!stoppingToken.IsCancellationRequested)
                {
                    var client = await _listener.AcceptTcpClientAsync(stoppingToken);
                    ForwardAsync(client, stoppingToken);


                    _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (SocketException e)
            {
                _logger.LogError("Socket exception: {Message}", e.Message);
                await Task.Delay(1000, stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error occurred: {Message}", e.Message);
                await Task.Delay(1000, stoppingToken);
            }
    }

    private async Task ForwardAsync(TcpClient client, CancellationToken stoppingToken)
    {
        try
        {
            var reader = new StreamReader(client.GetStream());

            while (await reader.ReadLineAsync(stoppingToken) is { } message)
            {
                _logger.LogInformation("[{Time}] Message: {Message}", DateTimeOffset.Now.ToString("u"), message);
                foreach (var hostPort in _destinationsSettings.Destinations)
                {
                    var dstEndPoint = new IPEndPoint(IPAddress.Parse(hostPort.Host), hostPort.Port);
                    using var dstClient = new TcpClient(dstEndPoint);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Unable to forward: {Message}", e.Message);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _listener.Stop();
        return base.StopAsync(cancellationToken);
    }
}