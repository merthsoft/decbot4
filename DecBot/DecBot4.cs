using Cemetech.DecBot4.ConsoleApp.MatterBridge;
using Cemetech.DecBot4.DecBot.Sql;
using Cemetech.DecBot4.MatterBridge;
using MySqlConnector;
using System.Data;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Cemetech.DecBot4.DecBot;

public partial class DecBot4
{
    private static readonly SocketsHttpHandler HttpClientHandler = new() { PooledConnectionLifetime = TimeSpan.FromHours(1) };

    [GeneratedRegex(@"\S+(?=\+\+)")]
    private static partial Regex CompiledPlusPlusRegex();
    private static readonly Regex PlusPlusRegex = CompiledPlusPlusRegex();

    private readonly DecBotConfig Config;

    public Action<string>? LogInformation { get; }
    public Action<string>? LogError { get; }
    public Action<string>? LogWarning { get; }
    public Action<string>? LogMessage { get; }

    private readonly DisposableConnection Connection;

    private readonly Dictionary<string, Func<string, string, bool, IList<string>, Task>> ChatCommandProcessors;

    public DecBot4(DecBotConfig config, Action<string>? logInformation = null, Action<string>? logError = null, Action<string>? logWarning = null, Action<string>? logMessage = null)
    {
        Config = config;
        LogInformation = logInformation;
        LogError = logError;
        LogWarning = logWarning;
        LogMessage = logMessage;

        string connectionString = Config.Database.ConnectionString
                        ?? $"Server={Config.Database.Server};" + 
                           $"Database={Config.Database.Database};" + 
                           $"Uid={Config.Database.User};" + 
                           $"Pwd={Config.Database.Password};" + 
                           $"Protocol={Config.Database.Protocol}";

        Connection = new DisposableConnection(new MySqlConnection(connectionString));

        ChatCommandProcessors = new() {
            // Karma stuff
            { "!karma", GetKarma },
            { "!kamra", GetKarma },
            { "!karm", GetKarma },
            { "!krama", GetKarma },
            { "!top", GetTop },
            { "!link", LinkNames },
            { "!unlink", UnlinkNames },
            { "!links", CheckLinks },
            { "!kerma", GetKerma },
            { "!total", GetTotals },
            // Quotes
            { "!q", GetQuote },
            { "!qsay", GetQuote },
            { "!quote", GetQuote },
            { "!qfind", GetQuote },
            { "!qsearch", GetQuote },
            { "!qadd", AddQuote },
            { "!quoteadd", AddQuote },
            { "!addquote", AddQuote },
            { "!qdel", DeleteQuote },
            { "!site", SaySite },
            // Privs
            { "!addprivs", AddPrivilegedUser }
        };
    }

    private async Task SendMessage(string channel, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var msg = new MessageToSend(
                Avatar: Config.Bot.Avatar,
                Gateway: channel,
                Text: message,
                Username: Config.Bot.Nickname);
            using var content = JsonContent.Create(msg, MessageToSendSourceGenerationContext.Default.MessageToSend);
            using var client = new HttpClient(handler: HttpClientHandler, false);
            client.BaseAddress = new(Config.MatterBridge.Host);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Config.MatterBridge.Token);

            LogMessage?.Invoke($"<<< [{msg.Username}] {msg.Text}");
            var response = await client.PostAsync("/api/message", content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                LogError?.Invoke($"Could not post message to {Config.MatterBridge.Host} /message. Status: {response.StatusCode} Body: {body}");
                return;
            }
        } catch (Exception ex)
        {
            LogError?.Invoke($"Exception posting message to {Config.MatterBridge.Host} /message: {ex}");
        }
    }

    private Task SendMessage(string channel, string message, params object[] format)
        => SendMessage(channel, string.Format(message, format));

    public async Task<bool> Initialize(CancellationToken cancellationToken = default)
    {
        try
        {
            LogInformation?.Invoke("Initializing database.");
            if (!await InitializeDatabase(cancellationToken))
            {
                LogInformation?.Invoke("Failed to initialize database.");
                return false;
            }

            LogInformation?.Invoke("Checking MatterBridge health.");
            if (!await CheckMatterBridge(cancellationToken))
            {
                LogInformation?.Invoke("MatterBridge is not up.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            LogError?.Invoke($"Error in initialize: {ex}");
            return false;
        }
    }

    private async Task<bool> InitializeDatabase(CancellationToken cancellationToken)
    {
        using (Connection.Open())
        using (var Command = new MySqlCommand(SharedSqlCommands.CountTotals, Connection))
        {
            using var reader = await Command.ExecuteReaderAsync(CommandBehavior.SingleResult, cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                LogInformation?.Invoke($"The total score is {reader["TotalScore"]} points between {reader["NameCount"]} names, with {reader["LinkCount"]} links. There are a total of {reader["TotalQuotes"]} quotes added by {reader["AddedByCount"]} people.");
            }
            else
            {
                LogError?.Invoke("Unable to connect to DB and read totals.");
                return false;
            }
        }

        return true;
    }
    private async Task<bool> CheckMatterBridge(CancellationToken cancellationToken)
    {
        using var client = new HttpClient(handler: HttpClientHandler, false);
        client.BaseAddress = new(Config.MatterBridge.Host);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Config.MatterBridge.Token);

        var response = await client.GetAsync("/api/health", cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            LogError?.Invoke($"Could not get /health from {Config.MatterBridge.Host}. Status: {response.StatusCode} Body: {body}");
            return false;
        } else
        {
            LogInformation?.Invoke($"MatterBridge replied 200: {body}");
        }

        return true;
    }

    public async Task ProcessMessageQueue(CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new HttpClient(handler: HttpClientHandler, false);
            client.BaseAddress = new(Config.MatterBridge.Host);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Config.MatterBridge.Token);
            
            var response = await client.GetAsync("/api/messages", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                LogError?.Invoke($"Could not get /messages from {Config.MatterBridge.Host}. Status: {response.StatusCode} Body: {body}");
                return;
            }

            using var contentStream = await response.Content.ReadAsStreamAsync();
            var messages = JsonSerializer.DeserializeAsyncEnumerable(contentStream, MessageReceivedSourceGenerationContext.Default.MessageReceived, cancellationToken);

            await foreach (var message in messages)
            {
                if (message == null || string.IsNullOrWhiteSpace(message.Text) || string.IsNullOrWhiteSpace(message.Username) || string.IsNullOrWhiteSpace(message.Gateway))
                    continue;

                var text = message.Text;
                var username = message.Username.Trim();
                var gateway = message.Gateway;

                LogMessage?.Invoke($">>> {username} {text}");

                try
                {
                    if (!string.IsNullOrWhiteSpace(Config.MatterBridge.NicknameRegex))
                        username = Regex.Match(username, Config.MatterBridge.NicknameRegex).Groups[1].Value;
                } catch (Exception ex)
                {
                    LogError?.Invoke($"Exception trying to parse username `{username}` with regex `{Config.MatterBridge.NicknameRegex}`: {ex}");
                }

                if (PlusPlusRegex.IsMatch(text))
                {
                    await HandlePlusPlus(gateway, username, text);
                }

                var split = text.Split(' ');
                var command = split[0];
                var parameters = split.Length > 1 ? split.Skip(1).ToArray() : [];
                
                if (ChatCommandProcessors.TryGetValue(command, out var func) && func != null)
                {
                    var hasPrivs = await GetPrivilegedUser(message.UserId, message.Protocol);
                    LogInformation?.Invoke($"User `{username}` on `{gateway}` invoked command `{command.ToString()}` (userId: `{message.UserId}` protocol: `{message.Protocol}` hasPrivs: `{hasPrivs}`)");
                    await func.Invoke(username, gateway, hasPrivs, parameters);
                }
            }
        }
        catch (Exception ex)
        {
            LogError?.Invoke($"Exception occurred while listening for messages. {ex}.");
        }
    }

    private async Task HandlePlusPlus(string channel, string messageSender, string message)
    {
        foreach (var matchGroup in PlusPlusRegex.Matches(message).Cast<Match>().GroupBy(m => m.Value))
        {
            var match = matchGroup.First();
            var change = matchGroup.Count();
            var name = match.Value.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(name)) { continue; }

            using (Connection.Open())
            using (var Command = new MySqlCommand() { Connection = Connection })
            {
                // Get the final name
                name = await getLinkedName(Command, name) ?? name;
                // See if we're the same person
                messageSender = await getLinkedName(Command, name) ?? messageSender;
                // Ignore if it's the same user
                if (messageSender.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    return;

                // Get the existing score
                var (_, score) = await getKarma(Command, name);
                score += 1;
                // Update the score
                var rows = await Command.ExecuteFullUpdateAsync(KarmaSqlCommands.InsertScore, ("@fname", name), ("@score", score));
                if (rows == 0)
                {
                    var error = string.Format("Could not update score for {0}.", name);
                    LogError?.Invoke(error);
                    await SendMessage(channel, error);
                    return;
                } else
                {
                    LogInformation?.Invoke($"User `{messageSender}` on gateway `{channel}` incremented the karma for `{name}` to `{score}`.");
                }
                // Track the score change
                rows = await Command.ExecuteFullUpdateAsync(KarmaSqlCommands.TrackScoreChange, ("@name", name), ("@change", change));
                if (rows == 0)
                {
                    var error = string.Format("Could not add tracking row for {0}.", name);
                    LogError?.Invoke(error);
                    await SendMessage(channel, error);
                }
            }
        }
    }
}
