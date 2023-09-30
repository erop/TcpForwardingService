using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Options;
using TcpForwardingService.Configuration;

namespace TcpForwardingService;

public class Worker : BackgroundService
{
    private readonly DestinationsSettings _destinationsSettings;
    private readonly ILogger<Worker> _logger;
    private readonly SourceSettings _sourceSettings;

    public Worker(ILogger<Worker> logger, IOptions<SourceSettings> sourceSettings,
        IOptions<DestinationsSettings> destinationsSettings)
    {
        _logger = logger;
        _sourceSettings = sourceSettings.Value;
        _destinationsSettings = destinationsSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
            try
            {
                var endpoint = new IPEndPoint(IPAddress.Parse(_sourceSettings.LocalIp), _sourceSettings.Port);
                var listener = new TcpListener(endpoint);
                listener.Start();
                _logger.LogInformation("Start listening on endpoint: {Endpoint}", endpoint.ToString());

                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
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
}