using Cemetech.DecBot4.DecBot;
using System.Text.Json;

namespace Cemetech.DecBot4;

internal class Program
{
    static void LogMessage(string message)
    {
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
        LogInformation("Started.");
        DecBotConfig? config = null;
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

            config = await JsonSerializer.DeserializeAsync(configStream, jsonTypeInfo: DecBotConfigSourceGenerationContext.Default.DecBotConfig);

            if (config == null)
            {
                LogError($"Could not deserialize config file {settingsLocation}.");
                return ErrorCodes.SettingsFileNotDeserialzed;
            }
        } catch (Exception ex) {
            LogError($"Exception reading config file {settingsLocation}. {ex}");
            return ErrorCodes.ExceptionReadingSettingsFile;
        }

        LogInformation("Initializing DecBot.");

        var decBot = new DecBot.DecBot4(config,
            logError: LogError,
            logInformation: LogInformation,
            logWarning: LogWarning,
            logMessage: LogMessage);

        if (!await decBot.Initialize())
        {
            LogError("Could not start DecBot.");
            return ErrorCodes.DecBotCouldNotInitialize;
        }

        LogInformation("DecBot initialized. Starting processing loop.");
        while (true)
        {
            try
            {
                await decBot.ProcessMessageQueue();
                await Task.Delay(config.Bot.ReadTimeout);
            } catch (Exception ex)
            {
                LogError($"Exception during message processing: {ex}");
            }
        }
    }
}
