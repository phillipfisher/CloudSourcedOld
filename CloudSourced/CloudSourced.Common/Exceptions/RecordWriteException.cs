using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudSourced.Exceptions
{
    public class RecordWriteException : CloudSourcedException
    {
        public RecordWriteException(Exception inner) : base("Error writing Event to storage!", inner) { }
    }
}
