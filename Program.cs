using System.Net;
using TcpForwardingService;
using TcpForwardingService.Configuration;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddOptions<SourceSettings>()
            .BindConfiguration(SourceSettings.Section)
            .Validate(
                settings => IPAddress.TryParse(settings.LocalIp, out _) &&
                            IsAllowedPort(settings.Port), "Incorrect source IP:PORT endpoint")
            .ValidateOnStart();

        services.AddOptions<DestinationsSettings>()
            .BindConfiguration(DestinationsSettings.Section)
            .Validate(
                settings =>
                {
                    return settings.Destinations.TrueForAll(hostPort =>
                        IPAddress.TryParse(hostPort.Host, out _) && IsAllowedPort(hostPort.Port));
                }, "Check destination IP:PORT endpoints!")
            .ValidateOnStart();
        
        services.AddOptions<RedisStreamSinkSettings>()
            .BindConfiguration(RedisStreamSinkSettings.Section)
            .Validate(settings => !string.IsNullOrWhiteSpace(settings.ConnectionString), "Check Redis connection string!")
            .Validate(settings => !string.IsNullOrWhiteSpace(settings.StreamName), "Check Redis stream name!")
            .ValidateOnStart();

        services.AddHostedService<Worker>();

        services.AddSingleton<TcpWritersPool>();

        services.AddWindowsService(options => options.ServiceName = "TcpForwardingService");
    })
    .Build();

host.Run();
return;

bool IsAllowedPort(int port)
{
    return port is > IPEndPoint.MinPort and < IPEndPoint.MaxPort;
}