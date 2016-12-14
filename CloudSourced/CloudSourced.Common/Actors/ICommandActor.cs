using Microsoft.ServiceFabric.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudSourced.Actors
{
    public interface ICommandActor : IActor
    {
        Task EnqueueItemAsync(CommandData command);
        Task EnqueueItemsAsync(IEnumerable<CommandData> commands);
        Task<IEnumerable<CommandContainer>> GetAllAsync();
        Task<IEnumerable<CommandContainer>> GetSinceVersionAsync(long versionId);
    }
}
