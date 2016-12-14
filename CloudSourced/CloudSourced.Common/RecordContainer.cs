using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CloudSourced
{
    [DataContract]
    public class RecordContainer
    {
        [DataMember]
        public readonly long VersionId;

        [DataMember]
        public readonly string Data;

        public RecordContainer(long versionId, string data)
        {
            VersionId = versionId;
            Data = data;
        }
    }
}
