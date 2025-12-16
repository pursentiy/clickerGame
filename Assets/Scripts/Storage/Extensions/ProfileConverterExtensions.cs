using System.Collections.Generic;
using System.Linq;
using Storage.Records;
using Storage.Snapshots;

namespace Storage.Extensions
{
    public static class ProfileConverterExtensions
    {
        public static ProfileRecord ToRecord(this ProfileSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return null;
            }
            
            var packSnapshots = snapshot.PackSnapshots == null ? new List<PackRecord>()
                : snapshot.PackSnapshots.Select(i => i.ToRecord()).Where(i => i != null).ToList();

            return new ProfileRecord(snapshot.Stars, packSnapshots);
        }
        
        public static PackRecord ToRecord(this PackSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return null;
            }
            
            var completedLevelsSnapshots = snapshot.CompletedLevelsSnapshots == null ? new List<LevelRecord>()
                : snapshot.CompletedLevelsSnapshots.Select(i => i.ToRecord()).Where(i => i != null).ToList();

            return new PackRecord(snapshot.PackNumber, completedLevelsSnapshots);
        }
        
        public static LevelRecord ToRecord(this LevelSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return null;
            }
        
            return new LevelRecord(snapshot.LevelNumber, snapshot.LevelCompletedTime, snapshot.StarsEarned);
        }
        
        public static ProfileSnapshot ToSnapshot(this ProfileRecord record)
        {
            if (record == null)
            {
                return null;
            }
            
            var packSnapshots = record.PackRecords == null ? new List<PackSnapshot>()
                : record.PackRecords.Select(i => i.ToSnapshot()).Where(i => i != null).ToList();

            return new ProfileSnapshot(record.Stars, packSnapshots);
        }

        public static PackSnapshot ToSnapshot(this PackRecord record)
        {
            if (record == null)
            {
                return null;
            }
            
            var completedLevelsSnapshots = record.CompletedLevelsRecords == null ? new List<LevelSnapshot>()
                : record.CompletedLevelsRecords.Select(i => i.ToSnapshot()).Where(i => i != null).ToList();

            return new PackSnapshot(record.PackNumber, completedLevelsSnapshots);
        }
        
        public static LevelSnapshot ToSnapshot(this LevelRecord record)
        {
            if (record == null)
            {
                return null;
            }
        
            return new LevelSnapshot(record.LevelNumber, record.LevelCompletedTime, record.StarsEarned);
        }
    }
}