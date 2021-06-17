using System;
using System.Runtime.InteropServices;

namespace LightVPN.Client.OpenVPN.Models
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct IoCounters
    {
        private readonly ulong ReadOperationCount;
        private readonly ulong WriteOperationCount;
        private readonly ulong OtherOperationCount;
        private readonly ulong ReadTransferCount;
        private readonly ulong WriteTransferCount;
        private readonly ulong OtherTransferCount;
    }
}