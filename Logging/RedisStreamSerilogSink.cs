using Serilog.Core;
using Serilog.Events;
using StackExchange.Redis;

namespace TcpForwardingService.Logging;

public class RedisStreamSerilogSink: ILogEventSink
{
    private readonly IDatabase _redis;

    public RedisStreamSerilogSink(IConnectionMultiplexer multiplexer)
    {
        _redis = multiplexer.GetDatabase();
    }

    public void Emit(LogEvent logEvent)
    {
        throw new NotImplementedException();
    }
}