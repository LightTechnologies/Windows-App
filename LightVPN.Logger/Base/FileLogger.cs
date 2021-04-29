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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightVPN.Logger.Base
{
    public abstract class FileLogger
    {
        private readonly string _fileName;
        public FileLogger(string fileName)
        {
            _fileName = fileName;
        }

        public string Read()
        {
            Verify();
            return File.ReadAllText(_fileName);
        }

        public async Task<string> ReadAsync()
        {
            Verify();
            return await File.ReadAllTextAsync(_fileName);
        }

        public void Write(string line)
        {
            Verify();
            File.AppendAllText(_fileName, $"[UTC: {DateTime.UtcNow}]: {line}\n");
        }

        public async Task WriteAsync(string line)
        {
            Verify();
            await File.AppendAllTextAsync(_fileName, $"[UTC: {DateTime.UtcNow}]: {line}\n");
        }

        public void Clear()
        {
            File.Delete(_fileName);
            Verify();
        }

        internal void Verify()
        {
            if (!Directory.Exists(Path.GetDirectoryName(_fileName)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_fileName));
            }
            if (!File.Exists(_fileName))
            {
                File.WriteAllText(_fileName, "██╗░░░░░██╗░██████╗░██╗░░██╗████████╗██╗░░░██╗██████╗░███╗░░██╗" +
                    "\n██║░░░░░██║██╔════╝░██║░░██║╚══██╔══╝██║░░░██║██╔══██╗████╗░██║" +
                    "\n██║░░░░░██║██║░░██╗░███████║░░░██║░░░╚██╗░██╔╝██████╔╝██╔██╗██║" +
                    "\n██║░░░░░██║██║░░╚██╗██╔══██║░░░██║░░░░╚████╔╝░██╔═══╝░██║╚████║" +
                    "\n███████╗██║╚██████╔╝██║░░██║░░░██║░░░░░╚██╔╝░░██║░░░░░██║░╚███║" +
                    "\n╚══════╝╚═╝░╚═════╝░╚═╝░░╚═╝░░░╚═╝░░░░░░╚═╝░░░╚═╝░░░░░╚═╝░░╚══╝\n\n");
            }
        }
    }
}
