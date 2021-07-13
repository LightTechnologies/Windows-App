namespace LightVPN.Client.Debug
{
    using System;
    using System.IO;

    public static class DebugLogger
    {
        public static readonly string DebugLogLocation = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LightVPN", "debug.log");

        public static void Write(string source, string log)
        {
            if (!Directory.Exists(Path.GetDirectoryName(DebugLogger.DebugLogLocation)))
                Directory.CreateDirectory(Path.GetDirectoryName(DebugLogger.DebugLogLocation)!);

            File.AppendAllText(DebugLogger.DebugLogLocation, $"[{source} at {DateTime.Now}]: {log}\r\n");
        }
    }
}
