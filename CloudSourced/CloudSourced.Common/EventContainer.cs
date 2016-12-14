using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CloudSourced
{
    [DataContract]
    public class EventContainer
    {
        [DataMember]
        public readonly long VersionId;

        [DataMember]
        public readonly EventData Data;

        public EventContainer(long versionId, EventData data)
        {
            VersionId = versionId;
            Data = data;
        }
    }
}
