using System.Text.Json.Serialization;

namespace TcpForwardingService.Logging;

public class Notification
{
    [JsonPropertyName("msgType")] public string MsgType { get; set; } = string.Empty;

    [JsonPropertyName("msgCategory")] public string MsgCategory { get; set; } = string.Empty;

    [JsonPropertyName("nodeId")] public int? NodeId { get; set; } = null;

    [JsonPropertyName("appType")] public string? AppType { get; set; } = null!;

    [JsonPropertyName("appId")] public string? AppId { get; set; } = null;

    [JsonPropertyName("msgPriority")] public int? MsgPriority { get; set; }

    [JsonPropertyName("companyId")] public int? CompanyId { get; set; }

    [JsonPropertyName("userId")] public int? UserId { get; set; } = null;

    [JsonPropertyName("msgText")] public string MsgText { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")] public long Timestamp { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds();
}