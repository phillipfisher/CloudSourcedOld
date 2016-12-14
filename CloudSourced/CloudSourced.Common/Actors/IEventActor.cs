using Microsoft.ServiceFabric.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudSourced.Actors
{
    public interface IEventActor : IActor
    {
        Task EnqueueItemAsync(EventData eventData);
        Task EnqueueItemsAsync(IEnumerable<EventData> events);
        Task<IEnumerable<EventContainer>> GetAllAsync();
        Task<IEnumerable<EventContainer>> GetSinceVersionAsync(long versionId);
    }
}
