using System;
using System.IO;

namespace LightVPN.Client.Debug
{
    public static class DebugLogger
    {
        private static readonly string _debugLogLocation = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LightVPN", "debug.log");

        public static void Write(string source, string log)
        {
            File.AppendAllText(_debugLogLocation, $"[{source} at {DateTime.Now}]: {log}\r\n");
        }
    }
}
