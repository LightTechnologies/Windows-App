namespace LightVPN.Client.OpenVPN.Utils
{
    using System.IO;

    /// <summary>
    ///     Handles writing data to the OpenVPN log
    /// </summary>
    internal sealed class LogDataManager
    {
        /// <summary>
        ///     The path to the OpenVPN log
        /// </summary>
        private readonly string _logDataPath;

        internal LogDataManager(string logDataPath)
        {
            this._logDataPath = logDataPath;
        }

        /// <summary>
        ///     Appends a object to the file, it will be converted to a string
        /// </summary>
        /// <param name="value">The value to write to the file</param>
        internal void WriteLine(object value)
        {
            File.AppendAllText(this._logDataPath, value + "\n");
        }
    }
}
