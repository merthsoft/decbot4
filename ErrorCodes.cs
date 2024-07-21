namespace Cemetech.DecBot4;
internal static class ErrorCodes
{
    internal static int Success = 0;
    internal static int SettingsFileNotFound = 1;
    internal static int SettingsFileNotRead = 2;
    internal static int SettingsFileNotDeserialzed = 3;
    internal static int ExceptionReadingSettingsFile = 4;
    internal static int DecBotCouldNotInitialize = 5;
}
