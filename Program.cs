using Cemetech.DecBot4.DecBot;
using System.Text.Json;

namespace Cemetech.DecBot4;

internal class Program
{
    static DecBotConfig DecBotConfig = null!; // Program will exit if this can't be initialized

    static void LogMessage(string message)
    {
        if (!DecBotConfig.Bot.LogMessages)
            return;
        var color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(message);
        Console.ForegroundColor = color;
    }

    static void LogError(string message)
    {
        var color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ForegroundColor = color;
    }

    static void LogWarning(string message)
    {
        var color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        Console.ForegroundColor = color;
    }

    static void LogInformation(string message)
    {
        var color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(message);
        Console.ForegroundColor = color;
    }

    static async Task<int> Main(string[] args)
    {
        LogInformation("Reading settings.");
        var settingsLocation = args.Length == 1 ? args[0] : "decbot.json";
        try
        {
            if (!File.Exists(settingsLocation))
            {
                LogError($"File not found {settingsLocation}.");
                return ErrorCodes.SettingsFileNotFound;
            }

            using var configStream = File.OpenRead(settingsLocation);
            if (configStream == null)
            {
                LogError($"Could not open settings file {settingsLocation}.");
                return ErrorCodes.SettingsFileNotRead;
            }

            DecBotConfig = (await JsonSerializer.DeserializeAsync(configStream, jsonTypeInfo: DecBotConfigSourceGenerationContext.Default.DecBotConfig))!;

            if (DecBotConfig == null)
            {
                LogError($"Could not deserialize config file {settingsLocation}.");
                return ErrorCodes.SettingsFileNotDeserialzed;
            }
        } catch (Exception ex) {
            LogError($"Exception reading config file {settingsLocation}. {ex}");
            return ErrorCodes.ExceptionReadingSettingsFile;
        }

        LogInformation("Initializing DecBot.");

        var decBot = new DecBot.DecBot4(DecBotConfig,
            logError: LogError,
            logInformation: LogInformation,
            logWarning: LogWarning,
            logMessage: LogMessage);

        if (!await decBot.Initialize())
        {
            LogError("Could not start DecBot.");
            return ErrorCodes.DecBotCouldNotInitialize;
        }

        LogInformation($"DecBot initialized. Starting processing loop with logMessages {DecBotConfig.Bot.LogMessages}");
        while (true)
        {
            try
            {
                await decBot.ProcessMessageQueue();
                await Task.Delay(DecBotConfig.Bot.ReadTimeout);
            } catch (Exception ex)
            {
                LogError($"Exception during message processing: {ex}");
            }
        }
    }
}
