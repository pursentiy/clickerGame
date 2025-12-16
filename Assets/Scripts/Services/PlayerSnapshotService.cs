using System.Collections.Generic;
using System.Linq;
using Extensions;
using Storage.Snapshots;

namespace Services
{
    public class PlayerSnapshotService
    {
        public ProfileSnapshot ProfileSnapshot { get; private set; }

        public void Initialize(ProfileSnapshot profileSnapshot)
        {
            ProfileSnapshot = profileSnapshot;
        }

        public bool HasLevelInPack(int packNumber, int levelNumber)
        {
            var pack = GetOrCreatePack(packNumber);

            var level = pack?.CompletedLevelsSnapshots.FirstOrDefault(x => x.LevelNumber == levelNumber);
            return level != null;
        }

        public PackSnapshot TryGetPack(int packNumber)
        {
            if (ProfileSnapshot == null || ProfileSnapshot.PackSnapshots.IsNullOrEmpty()) 
                return null;
            
            return ProfileSnapshot.PackSnapshots.FirstOrDefault(p => p.PackNumber == packNumber);
        }

        public PackSnapshot GetOrCreatePack(int packNumber)
        {
            if (ProfileSnapshot == null) 
                return null;

            ProfileSnapshot.PackSnapshots ??= new List<PackSnapshot>();

            var pack = ProfileSnapshot.PackSnapshots.FirstOrDefault(p => p.PackNumber == packNumber);
            if (pack != null) 
                return pack;
            
            pack = new PackSnapshot(packNumber, new List<LevelSnapshot>());
            ProfileSnapshot.PackSnapshots.Add(pack);

            return pack;
        }
    }
}