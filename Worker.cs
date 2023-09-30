using Microsoft.Extensions.Options;
using TcpForwardingService.Configuration;

namespace TcpForwardingService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly SourceSettings _sourceSettings;
    private readonly DestinationsSettings _destinationsSettings;

    public Worker(ILogger<Worker> logger, IOptions<SourceSettings> sourceSettings, IOptions<DestinationsSettings> destinationsSettings)
    {
        _logger = logger;
        _sourceSettings = sourceSettings.Value;
        _destinationsSettings = destinationsSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }
}