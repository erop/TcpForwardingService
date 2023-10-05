using System.Net;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Sinks.Slack;
using Serilog.Sinks.Slack.Models;
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

        services.AddOptions<SlackChannelSettings>()
            .BindConfiguration(SlackChannelSettings.Section)
            .ValidateOnStart();

        services.AddHostedService<Worker>();
        services.AddWindowsService(options => options.ServiceName = "TcpForwardingService");
        services.AddSingleton<TcpWritersPool>();
    })
    .UseSerilog((context, provider, loggerConfiguration) =>
    {
        var slackSettings = provider.GetRequiredService<IOptions<SlackChannelSettings>>().Value;
        loggerConfiguration
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.Slack(new SlackSinkOptions
            {
                CustomChannel = slackSettings.ChannelName,
                CustomUserName = slackSettings.UserName,
                WebHookUrl = slackSettings.WebHookUrl.ToString()
            });
    })
    .Build();

host.Run();
return;

bool IsAllowedPort(int port)
{
    return port is > IPEndPoint.MinPort and < IPEndPoint.MaxPort;
}