using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightVPN.Discord.Exceptions
{
    public class RpcException : Exception
    {
        public RpcException()
        {
        }

        public RpcException(string message)
            : base(message)
        {
        }

        public RpcException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
