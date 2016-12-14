using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace CloudSourced
{
    [DataContract]
    public class EventData
    {
        [DataMember]
        [JsonProperty("type")]
        public readonly string EventType;

        [DataMember]
        [JsonProperty("event")]
        public readonly string EventString;

        public EventData(string eventType, string eventString)
        {
            EventType = eventType;
            EventString = eventString;
        }

        public object ConvertEventToObject(Assemblies assemblies)
        {
            try
            {
                Type t = assemblies.GetType(EventType, true);
                return JsonConvert.DeserializeObject(EventString, t);
            }
            catch
            {
                return null;
            }
        }

        public static EventData Parse(object obj)
        {
            Type t = obj.GetType();
            string eventType = t.FullName;
            string eventString = JsonConvert.SerializeObject(obj);
            return new EventData(eventType, eventString);
        }
    }
}
