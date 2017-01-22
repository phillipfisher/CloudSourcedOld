using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CloudSourced
{
    [DataContract]
    [KnownType(typeof(Exception))]
    [KnownType(typeof(AggregateException))]
    [KnownType(typeof(ApplicationException))]
    [KnownType(typeof(ArgumentException))]
    [KnownType(typeof(ArgumentNullException))]
    [KnownType(typeof(ArgumentOutOfRangeException))]
    [KnownType(typeof(ArithmeticException))]
    [KnownType(typeof(BadImageFormatException))]
    [KnownType(typeof(DllNotFoundException))]
    [KnownType(typeof(IndexOutOfRangeException))]
    [KnownType(typeof(InsufficientMemoryException))]
    [KnownType(typeof(InvalidCastException))]
    [KnownType(typeof(InvalidDataContractException))]
    [KnownType(typeof(JsonException))]
    [KnownType(typeof(JsonSerializationException))]
    [KnownType(typeof(KeyNotFoundException))]
    [KnownType(typeof(NotImplementedException))]
    [KnownType(typeof(NotSupportedException))]
    [KnownType(typeof(NullReferenceException))]
    [KnownType(typeof(OutOfMemoryException))]
    [KnownType(typeof(OverflowException))]
    [KnownType(typeof(ReflectionTypeLoadException))]
    [KnownType(typeof(SystemException))]
    [KnownType(typeof(TimeoutException))]
    public class CommandData
    {
        [DataMember]
        [JsonProperty("type")]
        public readonly string CommandType;

        [DataMember]
        [JsonProperty("command")]
        public readonly string CommandString;

        [DataMember]
        [JsonProperty("events")]
        public readonly IEnumerable<Guid> AssociatedEvents;

        [DataMember]
        [JsonProperty("exceptions")]
        public readonly IEnumerable<Exception> ExceptionsThrown;

        public CommandData(string commandType, string commandString, IEnumerable<Guid> associatedEvents, IEnumerable<Exception> exceptionsThrown)
        {
            CommandType = commandType;
            CommandString = commandString;
            AssociatedEvents = associatedEvents;
            ExceptionsThrown = exceptionsThrown;
        }

        public object ConvertEventToObject(Assembly assembly)
        {
            try
            {
                Type t = assembly.GetType(CommandType);
                return JsonConvert.DeserializeObject(CommandString, t);
            }
            catch
            {
                return null;
            }
        }

        public static CommandData Parse(object obj, IEnumerable<Guid> associatedEvents, IEnumerable<Exception> exceptionsThrown)
        {
            Type t = obj.GetType();
            string commandType = t.FullName;
            string commandString = JsonConvert.SerializeObject(obj);
            return new CommandData(commandType, commandString, associatedEvents, exceptionsThrown);
        }
    }
}
