using Microsoft.Extensions.Options;
using Serilog.Core;
using Serilog.Events;
using StackExchange.Redis;
using TcpForwardingService.Configuration;

namespace TcpForwardingService.Logging;

public class RedisStreamSerilogSink: ILogEventSink
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
        throw new NotImplementedException();
    }
}