using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CloudSourced
{
    [DataContract]
    public class CommandContainer
    {
        [DataMember]
        public readonly long VersionId;

        [DataMember]
        public readonly CommandData Data;

        public CommandContainer(long versionId, CommandData data)
        {
            VersionId = versionId;
            Data = data;
        }
    }
}
