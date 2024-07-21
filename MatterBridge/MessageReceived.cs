using System.Text.Json.Serialization;

namespace Cemetech.DecBot4.ConsoleApp.MatterBridge;

[JsonSerializable(typeof(MessageReceived))]
[JsonSerializable(typeof(MessageReceived[]))]
public partial class MessageReceivedSourceGenerationContext : JsonSerializerContext
{
}

public record MessageReceived(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("channel")] string Channel,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("userid")] string UserId,
    [property: JsonPropertyName("avatar")] string Avatar,
    [property: JsonPropertyName("account")] string Account,
    [property: JsonPropertyName("event")] string Event,
    [property: JsonPropertyName("protocol")] string Protocol,
    [property: JsonPropertyName("gateway")] string Gateway,
    [property: JsonPropertyName("parent_id")] string ParentId,
    [property: JsonPropertyName("timestamp")] string Timestamp,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("Extra")] object Extra
);