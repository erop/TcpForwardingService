using System.Net;
using TcpForwardingService;
using TcpForwardingService.Configuration;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddOptions<SourceSettings>()
            .BindConfiguration(SourceSettings.Section)
            .Validate(
                settings =>
                {
                    return IPAddress.TryParse(settings.LocalIp, out _) &&
                           settings.Port is > IPEndPoint.MinPort and < IPEndPoint.MaxPort;
                }, "Incorrect source IP address")
            .ValidateOnStart();

        services.AddOptions<DestinationsSettings>()
            .BindConfiguration(DestinationsSettings.Section)
            .Validate(
                settings =>
                {
                    return settings.Destinations.TrueForAll(hostPort => IPAddress.TryParse(hostPort.Host, out _));
                }, "Check destination IP addresses!")
            .ValidateOnStart();

        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();