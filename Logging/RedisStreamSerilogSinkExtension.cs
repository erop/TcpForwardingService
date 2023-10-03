using Serilog;
using Serilog.Configuration;
using StackExchange.Redis;
using TcpForwardingService.Configuration;

namespace TcpForwardingService.Logging;

public static class RedisStreamSerilogSinkExtension
{
    public static LoggerConfiguration RedisStreamSink(this LoggerSinkConfiguration loggerConfiguration,
        RedisStreamSinkSettings settings)
    {
        var multiplexer = ConnectionMultiplexer.Connect(settings.ConnectionString);
        return loggerConfiguration.Sink(new RedisStreamSerilogSink(multiplexer));
    }
}