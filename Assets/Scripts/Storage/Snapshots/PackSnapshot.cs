using System.Collections.Generic;

namespace Storage.Snapshots
{
    public class PackSnapshot
    {
        public PackSnapshot(int packId, List<LevelSnapshot> completedLevelsSnapshots)
        {
            PackId = packId;
            CompletedLevelsSnapshots = completedLevelsSnapshots;
        }

        public int PackId {get; set;}
        public List<LevelSnapshot> CompletedLevelsSnapshots {get; set;}
        public UnlockStatus IsUnlocked {get; set;}
        public bool IsNew {get; set;}
    }
}