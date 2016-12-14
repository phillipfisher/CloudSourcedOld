using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using CloudSourced.Actors;

namespace CloudSourced.Event
{
    [StatePersistence(StatePersistence.Persisted)]
    class Event : SourcedActor, IEventActor
    {
        IEventActor AggregateClient = null;

        public Event(ActorService service, ActorId id) : base(service, id)
        {
        }

        protected override void GenerateAggregateClient()
        {
            if (DomainId != null)
                AggregateClient = Clients.CreateEventActor(ContainerName, DomainName, null);
        }

        protected override string GenerateFileName()
        {
            if (DomainId == null)
                return DomainName + ".aggregate.events";
            else
                return DomainName + "/" + DomainId + ".events";
        }

        #region IEvent methods
        async Task IEventActor.EnqueueItemAsync(EventData eventData)
        {
            await EnqueueAsync(new string[] { JsonConvert.SerializeObject(eventData) });

            if (AggregateClient != null)
                await AggregateClient.EnqueueItemAsync(eventData);
        }

        async Task IEventActor.EnqueueItemsAsync(IEnumerable<EventData> events)
        {
            await EnqueueAsync(events.Select(c => JsonConvert.SerializeObject(c)));

            if (AggregateClient != null)
                await AggregateClient.EnqueueItemsAsync(events);
        }

        async Task<IEnumerable<EventContainer>> IEventActor.GetAllAsync()
        {
            return (await GetAllAsync()).Select(r => new EventContainer(r.VersionId, JsonConvert.DeserializeObject<EventData>(r.Data)));
        }

        async Task<IEnumerable<EventContainer>> IEventActor.GetSinceVersionAsync(long versionId)
        {
            return (await GetSinceVersionAsync(versionId)).Select(r => new EventContainer(r.VersionId, JsonConvert.DeserializeObject<EventData>(r.Data)));
        }
        #endregion
    }
}
