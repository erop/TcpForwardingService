using System.Net;
using Microsoft.Extensions.Options;
using Serilog;
using StackExchange.Redis;
using TcpForwardingService;
using TcpForwardingService.Configuration;
using TcpForwardingService.Logging;

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
        services.AddWindowsService(options => options.ServiceName = "TcpForwardingService");
        
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<RedisStreamSinkSettings>>().Value;
            return ConnectionMultiplexer.Connect(settings.ConnectionString);
        });
        services.AddSingleton<TcpWritersPool>();

    })
    .UseSerilog((context, provider, loggerConfiguration) =>
    {
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.RedisStreamSink(provider.GetRequiredService<IConnectionMultiplexer>(),
                provider.GetRequiredService<IOptions<RedisStreamSinkSettings>>());
    })
    .Build();

host.Run();
return;

bool IsAllowedPort(int port)
{
    return port is > IPEndPoint.MinPort and < IPEndPoint.MaxPort;
}