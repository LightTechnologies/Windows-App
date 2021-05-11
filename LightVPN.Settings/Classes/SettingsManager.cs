/* --------------------------------------------
 *
 * Settings manager w/ generics - Main class
 * Copyright (C) Light Technologies LLC
 *
 * File: SettingsManager.cs
 *
 * Created: 04-03-21 Toshi
 *
 * --------------------------------------------
 */

using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using LightVPN.Settings.Interfaces;

namespace LightVPN.Settings
{
    /// <summary>
    /// Class used for saving an object to a file
    /// </summary>
    /// <typeparam name="T">The class to save to disk</typeparam>
    public class SettingsManager<T> : ISettingsManager<T>
    {
        private readonly string _path;

        /// <summary>
        /// Initalizes a new instance of the SettingsManager
        /// </summary>
        /// <param name="path">The path that the file will be stored in</param>
        public SettingsManager(string path) // this is truly a sexy class. It does not require any knowledge on the shit its saving thus producing loosely coupled model saving it can also save ANY model you can think pass into it
        {
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            _path = path;
        }

        /// <summary>
        /// Loads the type from the disk
        /// </summary>
        /// <returns>The class parsed from the json</returns>
        public T Load()
        {
            if (!File.Exists(_path))
            {
                File.WriteAllText(_path, "{}");
            }
            var text = File.ReadAllText(_path);
            var obj = JsonSerializer.Deserialize<T>(text);
            return obj;
        }

        /// <summary>
        /// Loads the type from the disk asynchronously
        /// </summary>
        /// <returns>The class returned</returns>
        public async Task<T> LoadAsync()
        {
            if (!File.Exists(_path))
            {
                await File.WriteAllTextAsync(_path, "");
            }
            var text = await File.ReadAllTextAsync(_path);
            var obj = JsonSerializer.Deserialize<T>(text);
            return obj;
        }

        /// <summary>
        /// Saves the T to disk synchronously
        /// </summary>
        /// <param name="type">The class to save to the disk</param>
        public void Save(T type)
        {
            var json = JsonSerializer.Serialize(type);
            File.WriteAllText(_path, json);
        }

        /// <summary>
        /// Saves the T to disk asynchronously
        /// </summary>
        /// <param name="type">The class to save to the disk</param>
        public async Task SaveAsync(T type)
        {
            var json = JsonSerializer.Serialize(type);
            await File.WriteAllTextAsync(_path, json);
        }
    }
}