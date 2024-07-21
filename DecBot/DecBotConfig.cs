using System.Text.Json.Serialization;

namespace Cemetech.DecBot4.DecBot;

[JsonSerializable(typeof(DecBotConfig))]
[JsonSerializable(typeof(BotConfig))]
[JsonSerializable(typeof(DatabaseConfig))]
[JsonSerializable(typeof(MatterBridgeConfig))]
public partial class DecBotConfigSourceGenerationContext : JsonSerializerContext
{
}

public record DecBotConfig(
    [property: JsonPropertyName("Database")] DatabaseConfig Database,
    [property: JsonPropertyName("MatterBridge")] MatterBridgeConfig MatterBridge,
    [property: JsonPropertyName("Bot")] BotConfig Bot
);

public record BotConfig(
    [property: JsonPropertyName("site")] string Site,
    [property: JsonPropertyName("readTimeout")] int ReadTimeout,
    [property: JsonPropertyName("nickname")] string Nickname,
    [property: JsonPropertyName("avatar")] string Avatar,
    [property: JsonPropertyName("logMessages")] bool LogMessages
);

public record DatabaseConfig(
    [property: JsonPropertyName("connectionString")] string? ConnectionString,
    [property: JsonPropertyName("server")] string Server,
    [property: JsonPropertyName("user")] string User,
    [property: JsonPropertyName("password")] string Password,
    [property: JsonPropertyName("database")] string Database,
    [property: JsonPropertyName("protool")] string Protocol = "socket"
);

public record MatterBridgeConfig(
    [property: JsonPropertyName("host")] string Host,
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("nicknameRegex")] string NicknameRegex
);