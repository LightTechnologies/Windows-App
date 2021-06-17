using System.IO;

namespace LightVPN.Client.OpenVPN.Utils
{
    internal class LogDataManager
    {
        private readonly string _logDataPath;

        internal LogDataManager(string logDataPath)
        {
            _logDataPath = logDataPath;
        }

        internal void WriteLine(object value)
        {
            File.WriteAllText(_logDataPath, value.ToString());
        }
    }
}