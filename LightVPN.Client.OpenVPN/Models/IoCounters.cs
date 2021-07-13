namespace LightVPN.Client.OpenVPN.Models
{
    using System.Runtime.InteropServices;

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
