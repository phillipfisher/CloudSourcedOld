using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudSourced.Actors
{
    public static class Clients
    {
        public readonly static Uri CommandUri = new Uri("fabric:/CloudSourcedApp/CommandActorService");
        public readonly static Uri EventUri = new Uri("fabric:/CloudSourcedApp/EventActorService");

        public static ICommandActor CreateCommandActor(string containerName, string domainName, string domainId)
        {
            ValidateInputs(containerName, domainName, domainId);
            if (domainId == null)
                return ActorProxy.Create<ICommandActor>(new ActorId(domainName + "@" + containerName), CommandUri);
            return ActorProxy.Create<ICommandActor>(new ActorId(domainName + "/" + domainId + "@" + containerName), CommandUri);
        }

        public static IEventActor CreateEventActor(string containerName, string domainName, string domainId)
        {
            ValidateInputs(containerName, domainName, domainId);
            if (domainId == null)
                return ActorProxy.Create<IEventActor>(new ActorId(domainName + "@" + containerName), EventUri);
            return ActorProxy.Create<IEventActor>(new ActorId(domainName + "/" + domainId + "@" + containerName), EventUri);
        }

        private static void ValidateInputs(string containerName, string domainName, string domainId)
        {
            if (string.IsNullOrWhiteSpace(containerName))
                throw new ArgumentNullException(nameof(containerName));
            if (string.IsNullOrWhiteSpace(domainName))
                throw new ArgumentNullException(nameof(domainName));
            if (containerName.Contains('@') || containerName.Contains('/'))
                throw new ArgumentException("Cannot use @ or / symbols.", nameof(containerName));
            if (domainName.Contains('@') || domainName.Contains('/'))
                throw new ArgumentException("Cannot use @ or / symbols.", nameof(domainName));

            if (domainId == null)
                return;
            if (domainId.Contains('@') || domainId.Contains('/'))
                throw new ArgumentException("Cannot use @ or / symbols.", nameof(domainId));
        }
    }
}
