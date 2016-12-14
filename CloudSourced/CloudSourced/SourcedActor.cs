using CloudSourced.Exceptions;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudSourced
{
    [StatePersistence(StatePersistence.Persisted)]
    abstract class SourcedActor : Actor, IActor
    {
        protected string ContainerName;
        protected string DomainName;
        protected string DomainId;
        protected string Filename;

        PersistedVariable<SourcedState> StateVar;

        CloudBlobContainer container;
        CloudAppendBlob file;

        #region Init methods
        public SourcedActor(ActorService service, ActorId id) : base(service, id) { }

        protected sealed override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            ParseActorId();
            GenerateAggregateClient();

            container = await ContainerManager.Get(ContainerName);
            file = container.GetAppendBlobReference(Filename);

            StateVar = new PersistedVariable<SourcedState>(StateManager, "state", LoadSourcedStateFromScratch);
        }

        async Task<SourcedState> LoadSourcedStateFromScratch()
        {
            SourcedState state = new SourcedState();

            if (!await file.ExistsAsync())
                return state;

            var records = await GetAllAsync();
            var dict = new Dictionary<long, RecordContainer>();
            foreach (var e in records)
                dict[e.VersionId] = e;

            if (dict.Count > 0)
                state.AddNewRecords(dict);

            return state;
        }
        #endregion

        #region Main methods
        protected async Task EnqueueAsync(IEnumerable<string> records)
        {
            try
            {
                using (StateVar)
                {
                    SourcedState state = await StateVar.GetAsync();

                    if (!await file.ExistsAsync())
                        await file.CreateOrReplaceAsync();

                    long currentVersion = state.CurrentVersion;
                    StringBuilder sb = new StringBuilder();
                    Dictionary<long, RecordContainer> containers = new Dictionary<long, RecordContainer>();
                    foreach (var data in records)
                    {
                        //Create the container in memory
                        RecordContainer container = new RecordContainer(++currentVersion, data.Replace("\r", "").Replace("\n", ""));
                        containers.Add(container.VersionId, container);

                        //Test if need to create pointer
                        if (currentVersion % SourcedState.PointersToRecordsChunkSize == 0)
                            state.PointersToRecords.Add(currentVersion, await GetFileSize());

                        //Create string for storage
                        sb.Append(container.VersionId + ",");
                        sb.Append(container.Data);
                        sb.Append("\n");

                    }

                    //Write it to storage, this only stays congruent because this is single threaded
                    await file.AppendTextAsync(sb.ToString());

                    //Add records to the state and update CurrentVersion
                    state.AddNewRecords(containers);
                }
            }
            catch (Exception e)
            {
                throw new RecordWriteException(e);
            }

        }

        protected async Task<IEnumerable<RecordContainer>> GetAllAsync()
        {
            return await GetAllFromPointerAsync(0);
        }

        protected async Task<IEnumerable<RecordContainer>> GetSinceVersionAsync(long versionId)
        {
            var state = await StateVar.GetAsync();
            if (state.ReplayCache.Any(e => e.Key == versionId))
                return state.ReplayCache.Where(e => e.Key >= versionId).OrderBy(e => e.Key).Select(e => e.Value);

            return (await GetAllAsync()).Where(e => e.VersionId >= versionId);
        }
        #endregion

        /// <summary>
        /// Gets all records from a starting file pointer
        /// </summary>
        /// <param name="pointer">Byte position inside the file to start downloading from.</param>
        /// <returns></returns>
        private async Task<IEnumerable<RecordContainer>> GetAllFromPointerAsync(long pointer)
        {
            if (!await file.ExistsAsync())
                return new List<RecordContainer>();
            if ((await GetFileSize()) <= pointer)
                return new List<RecordContainer>();

            using (MemoryStream ms = new MemoryStream())
            {
                await file.DownloadRangeToStreamAsync(ms, pointer, null);
                ms.Position = 0;

                return ParseRecordStream(ms);
            }

        }


        private async Task<long> GetFileSize()
        {
            await file.FetchAttributesAsync();
            return file.Properties.Length;
        }

        protected abstract void GenerateAggregateClient();
        protected abstract string GenerateFileName();

        private void ParseActorId()
        {
            string actorId = this.GetActorId().GetStringId();
            string[] pieces = actorId.Split('@');
            if (pieces.Length != 2)
                throw new Exception("UserService: Invalid ActorId = " + actorId);
            ContainerName = pieces[1];
            string domainPortion = pieces[0];

            if (domainPortion.Contains('/'))
            {
                pieces = domainPortion.Split('/');
                if (pieces.Length != 2)
                    throw new Exception("UserService: Invalid ActorId = " + actorId);
                DomainName = pieces[0];
                DomainId = pieces[1];
            }
            else
            {
                DomainName = domainPortion;
                DomainId = null;
            }
            Filename = GenerateFileName();
        }

        private IEnumerable<RecordContainer> ParseRecordStream(MemoryStream stream)
        {
            List<RecordContainer> records = new List<RecordContainer>();
            using (StreamReader sr = new StreamReader(stream))
            {
                while (sr.Peek() >= 0)
                {
                    string line = sr.ReadLine();
                    int i = line.IndexOf(',');
                    if (i < 1)
                        break;

                    long versionId;
                    if (!long.TryParse(line.Substring(0, i), out versionId))
                        break;

                    records.Add(new RecordContainer(versionId, line.Substring(i + 1)));
                }
            }

            return records;
        }
    }
}
