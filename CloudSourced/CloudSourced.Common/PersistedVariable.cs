using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CloudSourced
{
    public class PersistedVariable<T> : IDisposable
    {
        IActorStateManager stateManager;
        string stateName;
        Func<Task<T>> factory;
        T value;
        bool loaded = false;
        int version;

        private string VersionStateName { get { return stateName + "_PersistedVariableVersion"; } }

        public PersistedVariable(IActorStateManager stateManager, string stateName, int version = 1)
        {
            if (stateManager == null)
                throw new ArgumentNullException("stateManager");

            if (string.IsNullOrWhiteSpace(stateName))
                throw new ArgumentNullException("stateName");

            this.stateManager = stateManager;
            this.stateName = stateName;
            this.version = version;
        }

        public PersistedVariable(IActorStateManager stateManager, string stateName, T defaultValue, int version = 1) : this(stateManager, stateName, version)
        {
            this.factory = () => { return Task.FromResult(defaultValue); };
        }

        public PersistedVariable(IActorStateManager stateManager, string stateName, Func<Task<T>> factory, int version = 1) : this(stateManager, stateName, version)
        {
            this.factory = factory;
        }

        public async Task<T> GetAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!loaded)
            {
                await LoadValueFromStateManager(cancellationToken);
            }
            return value;
        }

        private async Task LoadValueFromStateManager(CancellationToken cancellationToken)
        {
            ConditionalValue<int> versionValue = await stateManager.TryGetStateAsync<int>(VersionStateName, cancellationToken);
            if (versionValue.HasValue && versionValue.Value != this.version)
            {
                await stateManager.TryRemoveStateAsync(stateName, cancellationToken);
                await stateManager.TryRemoveStateAsync(VersionStateName, cancellationToken);
            }

            ConditionalValue<T> condValue = await stateManager.TryGetStateAsync<T>(stateName, cancellationToken);
            if (condValue.HasValue)
                value = condValue.Value;
            else if (factory != null)
                value = await factory();
            else
                value = default(T);

            loaded = true;
        }

        public async Task SaveAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (loaded)
            {
                await stateManager.SetStateAsync(VersionStateName, version, cancellationToken);
                await stateManager.SetStateAsync(stateName, value, cancellationToken);
            }
        }

        public async Task<T> SetAsync(T valueIn, CancellationToken cancellationToken = default(CancellationToken))
        {
            loaded = true;
            value = valueIn;
            await SaveAsync(cancellationToken);
            return valueIn;
        }

        void IDisposable.Dispose()
        {
            SaveAsync().Wait();
        }
    }
}
