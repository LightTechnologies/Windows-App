namespace LightVPN.Client.OpenVPN.Models
{
    using System;
    using System.Runtime.InteropServices;

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
