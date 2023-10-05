namespace TcpForwardingService.Configuration;

public class SlackChannelSettings
{
   public const string Section = "Slack";
   public required Uri WebHookUrl { get; set; }
   public required string ChannelName { get; set; }
   public required string UserName { get; set; }
   public required string MinimumLogLevel { get; set; }
}