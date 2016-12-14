using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Newtonsoft.Json;
using CloudSourced.Actors;

namespace CloudSourced.Command
{
    [StatePersistence(StatePersistence.Persisted)]
    class Command : SourcedActor, ICommandActor
    {
        ICommandActor AggregateClient = null;

        public Command(ActorService service, ActorId id) : base(service, id)
        {
        }

        protected override void GenerateAggregateClient()
        {
            if (DomainId != null)
                AggregateClient = Clients.CreateCommandActor(ContainerName, DomainName, null);
        }

        protected override string GenerateFileName()
        {
            if (DomainId == null)
                return DomainName + ".aggregate.commands";
            else
                return DomainName + "/" + DomainId + ".commands";
        }

        #region IEvent methods
        async Task ICommandActor.EnqueueItemAsync(CommandData command)
        {
            await EnqueueAsync(new string[] { JsonConvert.SerializeObject(command) });

            if (AggregateClient != null)
                await AggregateClient.EnqueueItemAsync(command);
        }

        async Task ICommandActor.EnqueueItemsAsync(IEnumerable<CommandData> commands)
        {

            await EnqueueAsync(commands.Select(c => JsonConvert.SerializeObject(c)));

            if (AggregateClient != null)
                await AggregateClient.EnqueueItemsAsync(commands);
        }

        async Task<IEnumerable<CommandContainer>> ICommandActor.GetAllAsync()
        {
            return (await GetAllAsync()).Select(r => new CommandContainer(r.VersionId, JsonConvert.DeserializeObject<CommandData>(r.Data)));
        }

        async Task<IEnumerable<CommandContainer>> ICommandActor.GetSinceVersionAsync(long versionId)
        {
            return (await GetSinceVersionAsync(versionId)).Select(r => new CommandContainer(r.VersionId, JsonConvert.DeserializeObject<CommandData>(r.Data)));
        }
        #endregion
    }
}
