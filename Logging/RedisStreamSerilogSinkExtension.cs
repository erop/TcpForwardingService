using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Configuration;
using StackExchange.Redis;
using TcpForwardingService.Configuration;

namespace TcpForwardingService.Logging;

public static class RedisStreamSerilogSinkExtension
{
    public static LoggerConfiguration RedisStreamSink(this LoggerSinkConfiguration loggerConfiguration,
        IConnectionMultiplexer multiplexer, IOptions<RedisStreamSinkSettings> options)
    {
        return loggerConfiguration.Sink(new RedisStreamSerilogSink(multiplexer, options.Value));
    }
}