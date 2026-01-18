using System.Collections.Generic;
using System.Linq;
using Common.Currency;
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

            //TODO FIX CONVERSION
            return new ProfileRecord(
                snapshot.Stars,
                (int)snapshot.HardCurrency.GetCount(),
                (int)snapshot.SoftCurrency.GetCount(),
                packSnapshots,
                snapshot.AnalyticsInfoSnapshot.ToRecord(),
                snapshot.GameParamsSnapshot.ToRecord());
        }
        
        public static PackRecord ToRecord(this PackSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return null;
            }
            
            var completedLevelsSnapshots = snapshot.CompletedLevelsSnapshots == null ? new List<LevelRecord>()
                : snapshot.CompletedLevelsSnapshots.Select(i => i.ToRecord()).Where(i => i != null).ToList();

            return new PackRecord(snapshot.PackId, completedLevelsSnapshots, snapshot.IsUnlocked);
        }
        
        public static LevelRecord ToRecord(this LevelSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return null;
            }
        
            return new LevelRecord(snapshot.LevelId, snapshot.BestCompletedTime, snapshot.StarsEarned, snapshot.IsUnlocked, snapshot.PlayCount);
        }
        
        public static AnalyticsInfoRecord ToRecord(this AnalyticsInfoSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return null;
            }
        
            return new AnalyticsInfoRecord(snapshot.LastSaveTime, snapshot.TotalPlayTime, snapshot.CreationDate);
        }
        
        public static AnalyticsInfoSnapshot ToSnapshot(this AnalyticsInfoRecord record)
        {
            if (record == null)
            {
                return null;
            }

            return new AnalyticsInfoSnapshot(record.LastSaveTime, record.TotalPlayTime, record.CreationDate);
        }

        public static GameParamsSnapshot ToSnapshot(this GameParamsRecord record)
        {
            if (record == null)
                return null;
            
            return new GameParamsSnapshot(record.IsMusicOn, record.IsSoundOn, record.Language);
        }
        
        public static GameParamsRecord ToRecord(this GameParamsSnapshot snapshot)
        {
            if (snapshot == null)
                return null;
            
            return new GameParamsRecord(snapshot.IsMusicOn, snapshot.IsSoundOn, snapshot.Language);
        }
        
        public static ProfileSnapshot ToSnapshot(this ProfileRecord record)
        {
            if (record == null)
            {
                return null;
            }
            
            var packSnapshots = record.PackRecords == null ? new List<PackSnapshot>()
                : record.PackRecords.Select(i => i.ToSnapshot()).Where(i => i != null).ToList();

            //TODO beware of overflow
            return new ProfileSnapshot(
                new Stars(record.Stars),
                new SoftCurrency(record.SoftCurrency),
                new HardCurrency(record.HardCurrency),
                packSnapshots,
                record.PurchasedItemsIds,
                record.AnalyticsInfoRecord.ToSnapshot(),
                record.GameParamsRecord.ToSnapshot());
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
        
            //TODO beware of overflow
            return new LevelSnapshot(record.LevelNumber, record.BestCompletedTime, new Stars(record.StarsEarned), record.IsUnlocked, record.PlayCount);
        }
    }
}