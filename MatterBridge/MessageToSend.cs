using System.Text.Json.Serialization;

namespace Cemetech.DecBot4.MatterBridge;

[JsonSerializable(typeof(MessageToSend))]
public partial class MessageToSendSourceGenerationContext : JsonSerializerContext
{
}

public record MessageToSend (
    [property: JsonPropertyName("avatar")] string Avatar,
    [property: JsonPropertyName("gateway")] string Gateway,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("event")] string Event = ""
);