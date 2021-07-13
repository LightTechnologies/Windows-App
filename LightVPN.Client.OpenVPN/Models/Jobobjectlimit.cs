namespace LightVPN.Client.OpenVPN.Models
{
    using System;

    [Flags]
    internal enum Jobobjectlimit : uint
    {
        JobObjectLimitKillOnJobClose = 0x2000,
    }
}
