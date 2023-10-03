namespace TcpForwardingService.Configuration;

public class RedisStreamSinkSettings
{
    public const string Section = "Redis";
    public required string ConnectionString { get; set; }
    public required string StreamName { get; set; }
}