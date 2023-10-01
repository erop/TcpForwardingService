using System.ComponentModel.DataAnnotations;

namespace TcpForwardingService.Configuration;

public class DestinationsSettings
{
    public const string Section = "Destinations";

    [Required] public List<HostPort> Destinations { get; set; } = new();
}

public class HostPort
{
    [Required] public required string Host { get; set; }

    [Required] public int Port { get; set; }

    public override string ToString()
    {
        return $"{Host}:{Port}";
    }
}