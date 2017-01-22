using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
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

        public object ConvertEventToObject(params Assembly[] assemblies)
        {
            try
            {
                Assemblies a = new Assemblies(assemblies);
                Type t = a.GetType(EventType, true);
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
                };

                return JsonConvert.DeserializeObject(EventString, t, settings);
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
