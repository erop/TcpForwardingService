using System.Text.Json;
using Serilog.Core;
using Serilog.Events;
using StackExchange.Redis;
using TcpForwardingService.Configuration;

namespace TcpForwardingService.Logging;

public class RedisStreamSerilogSink : ILogEventSink
{
    private readonly IDatabase _redis;
    private readonly RedisStreamSinkSettings _settings;

    public RedisStreamSerilogSink(IConnectionMultiplexer multiplexer, RedisStreamSinkSettings settings)
    {
        _redis = multiplexer.GetDatabase();
        _settings = settings;
    }

    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage();
        var level = logEvent.Level.ToString();
        var timestamp = logEvent.Timestamp.ToUnixTimeMilliseconds();
        var properties = logEvent.Properties;

        var notification = new Notification
        {
            AppId = "TcpForwardingService",
            AppType = null,
            MsgType = "TcpForwardingService Error",
            MsgCategory = "Application_Alert",
            CompanyId = null,
            Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
            MsgPriority = 1,
            MsgText = message
        };

        _redis.StreamAdd(_settings.StreamName, new NameValueEntry[]
        {
            new("id", notification.Timestamp),
            new("payload", JsonSerializer.Serialize(notification))
        });
    }
}