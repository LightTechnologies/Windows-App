using System;

namespace LightVPN.Client.OpenVPN.Models
{
    [Flags]
    internal enum Jobobjectlimit : uint
    {
        JobObjectLimitKillOnJobClose = 0x2000
    }
}