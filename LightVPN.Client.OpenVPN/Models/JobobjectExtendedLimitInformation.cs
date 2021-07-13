namespace LightVPN.Client.OpenVPN.Models
{
    using System;
    using System.Runtime.InteropServices;

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
