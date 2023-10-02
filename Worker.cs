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
    private readonly TcpWritersPool _writersPool;

    public Worker(ILogger<Worker> logger, IOptions<SourceSettings> sourceSettings,
        IOptions<DestinationsSettings> destinationsSettings, TcpWritersPool writersPool)
    {
        _logger = logger;
        _writersPool = writersPool;
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
                    _ = ForwardAsync(client, stoppingToken);


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
            using (client)
            {
                var reader = new StreamReader(client.GetStream());

                while (await reader.ReadLineAsync(stoppingToken) is { } message)
                {
                    _logger.LogInformation("[{Time}] Message: {Message}", DateTimeOffset.Now.ToString("u"), message);
                    foreach (var (key, writer) in _writersPool.GetWriters())
                        if (writer is not null)
                        {
                            try
                            {
                                await writer.WriteLineAsync(message);
                                await writer.FlushAsync();
                            }
                            catch (Exception e) when (e is SocketException or ObjectDisposedException
                                                          or InvalidOperationException)
                            {
                                _logger.LogError(
                                    "Unable to forward message: '{Message}' to destination '{Destination}'",
                                    e.Message, key.ToString());
                                ReEstablishConnection(key);
                            }
                            catch (Exception e)
                            {
                                _logger.LogCritical("Unexpected error occurred while forwarding message: {Message}",
                                    e.Message);
                                ReEstablishConnection(key);
                            }
                        }
                        else
                        {
                            _logger.LogError("No available connection for endpoint '{Endpoint}'", key.ToString());
                            ReEstablishConnection(key);
                        }
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Unable to forward: {Message}", e.Message);
        }
    }

    private void ReEstablishConnection(IPEndPoint ipEndPoint)
    {
        Task.Run(() => _writersPool.AddWriter(ipEndPoint)).ConfigureAwait(false);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _listener.Stop();
        _writersPool.Dispose();
        return base.StopAsync(cancellationToken);
    }
}