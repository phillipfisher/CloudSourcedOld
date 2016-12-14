using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudSourced.Exceptions
{
    public class CloudSourcedException : Exception
    {
        public CloudSourcedException(string message) : base(message) {}

        public CloudSourcedException(string message, Exception inner) : base(message, inner) { }
    }
}
