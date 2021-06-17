using System;
using System.Runtime.InteropServices;
using LightVPN.Client.OpenVPN.Utils;

namespace LightVPN.Client.OpenVPN.Models
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct JobobjectExtendedLimitInformation
    {
        public JobobjectBasicLimitInformation BasicLimitInformation;
        private readonly IoCounters IoInfo;
        private readonly UIntPtr ProcessMemoryLimit;
        private readonly UIntPtr JobMemoryLimit;
        private readonly UIntPtr PeakProcessMemoryUsed;
        private readonly UIntPtr PeakJobMemoryUsed;
    }
}