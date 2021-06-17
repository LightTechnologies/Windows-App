using System;
using System.Runtime.InteropServices;
using LightVPN.Client.OpenVPN.Utils;

namespace LightVPN.Client.OpenVPN.Models
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct JobobjectBasicLimitInformation
    {
        private readonly long PerProcessUserTimeLimit;
        private readonly long PerJobUserTimeLimit;
        internal Jobobjectlimit LimitFlags;
        private readonly UIntPtr MinimumWorkingSetSize;
        private readonly UIntPtr MaximumWorkingSetSize;
        private readonly uint ActiveProcessLimit;
        private readonly long Affinity;
        private readonly uint PriorityClass;
        private readonly uint SchedulingClass;
    }
}