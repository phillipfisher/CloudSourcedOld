using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace CloudSourced
{
    [DataContract]
    class SourcedState
    {
        /// <summary>
        /// How often should a pointer be remembered
        /// </summary>
        public const int PointersToRecordsChunkSize = 10;

        /// <summary>
        /// Number of Records to keep in memory
        /// </summary>
        public const int ReplayCacheSize = 10;

        /// <summary>
        /// Tracks the current version (last Id saved)
        /// </summary>
        [DataMember]
        public long CurrentVersion;

        /// <summary>
        /// An in memory cache of the last x records (where x is less than or equal to ReplayCacheSize). The key is the version Id of the record.
        /// </summary>
        [DataMember]
        public IEnumerable<KeyValuePair<long, RecordContainer>> ReplayCache;

        /// <summary>
        /// List of pointers to records.  The pointers are the number of bytes to the beginning of that record.
        /// There is one for every PointersToRecordsChunkSize. The key is the version Id of the record.
        /// </summary>
        [DataMember]
        public Dictionary<long, long> PointersToRecords;

        public SourcedState()
        {
            CurrentVersion = 0;
            ReplayCache = new Dictionary<long, RecordContainer>();
            PointersToRecords = new Dictionary<long, long>();
        }

        internal void AddNewRecords(IEnumerable<KeyValuePair<long, RecordContainer>> records)
        {
            ReplayCache = ReplayCache.Concat(records).OrderByDescending(e => e.Key).Take(ReplayCacheSize);
            CurrentVersion = ReplayCache.First().Value.VersionId;
        }

        internal long GetBestPointer(long versionId)
        {
            long nextLowestVersionId = versionId - (versionId % PointersToRecordsChunkSize);

            long nextLowestPointer;
            if (PointersToRecords.TryGetValue(nextLowestVersionId, out nextLowestPointer))
                return nextLowestPointer;

            return 0;
        }
    }
}
