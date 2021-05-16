/* --------------------------------------------
 *
 * File logger - Base class
 * Copyright (C) Light Technologies LLC
 *
 * File: FileLogger.cs
 *
 * Created: 04-03-21 Khrysus
 *
 * --------------------------------------------
 */

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LightVPN.FileLogger.Base
{
    /// <summary>
    /// Base class that all the file loggers derive from
    /// </summary>
    public abstract class FileLoggerBase
    {
        private readonly string _fileName;

        /// <summary>
        /// Constructs the FileLogger base class
        /// </summary>
        /// <param name="fileName">The location of the log file that this class will use</param>
        public FileLoggerBase(string fileName) => _fileName = fileName;

        /// <summary>
        /// Deletes the log file and re-creates it.
        /// </summary>
        public void Clear()
        {
            File.Delete(_fileName);
            Verify();
        }

        /// <summary>
        /// Reads the file specified in the constructor
        /// </summary>
        /// <returns></returns>
        public string Read()
        {
            Verify();
            return File.ReadAllText(_fileName);
        }

        /// <summary>
        /// Reads the file specified in the constructor asynchronously
        /// </summary>
        /// <returns>The contents of the file</returns>
        public async Task<string> ReadAsync(CancellationToken cancellationToken = default)
        {
            Verify();
            return await File.ReadAllTextAsync(_fileName, cancellationToken);
        }

        /// <summary>
        /// Writes a line to the file specified in the constructor. Newlines and dates are added automatically
        /// </summary>
        /// <param name="line">The line you want to write to the file</param>
        public void Write(string line)
        {
            Verify();
            File.AppendAllText(_fileName, $"[UTC: {DateTime.UtcNow}]: {line}\n");
        }

        /// <summary>
        /// Writes a line to the file specified in the constructor asynchronously. Newlines and
        /// dates are added automatically
        /// </summary>
        /// <param name="line">The line you want to write to the file</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task WriteAsync(string line, CancellationToken cancellationToken = default)
        {
            Verify();
            await File.AppendAllTextAsync(_fileName, $"[UTC: {DateTime.UtcNow}]: {line}\n", cancellationToken);
        }

        /// <summary>
        /// Verifies the existance of the log file, and if it doesn't exist or the directory doesn't
        /// exist then it creates them.
        /// </summary>
        internal void Verify()
        {
            if (!Directory.Exists(Path.GetDirectoryName(_fileName))) Directory.CreateDirectory(Path.GetDirectoryName(_fileName));
            if (!File.Exists(_fileName))
            {
                File.WriteAllText(_fileName, @"██╗░░░░░██╗░██████╗░██╗░░██╗████████╗██╗░░░██╗██████╗░███╗░░██╗
                    ██║░░░░░██║██╔════╝░██║░░██║╚══██╔══╝██║░░░██║██╔══██╗████╗░██║
                    ██║░░░░░██║██║░░██╗░███████║░░░██║░░░╚██╗░██╔╝██████╔╝██╔██╗██║
                    ██║░░░░░██║██║░░╚██╗██╔══██║░░░██║░░░░╚████╔╝░██╔═══╝░██║╚████║
                    ███████╗██║╚██████╔╝██║░░██║░░░██║░░░░░╚██╔╝░░██║░░░░░██║░╚███║
                    ╚══════╝╚═╝░╚═════╝░╚═╝░░╚═╝░░░╚═╝░░░░░░╚═╝░░░╚═╝░░░░░╚═╝░░╚══╝");
            }
        }
    }
}