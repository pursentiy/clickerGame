using System.Collections.Generic;
using Configurations.Progress;

namespace Storage.Snapshots
{
    public class PackSnapshot
    {
        public PackSnapshot(int packId, List<LevelSnapshot> completedLevelsSnapshots, PackType packType = PackType.Default)
        {
            PackId = packId;
            CompletedLevelsSnapshots = completedLevelsSnapshots;
            PackType = packType; // Defaults to PackType.Default (0) for existing DB values
        }

        public int PackId {get; set;}
        public List<LevelSnapshot> CompletedLevelsSnapshots {get; set;}
        public UnlockStatus IsUnlocked {get; set;}
        public bool IsNew {get; set;}
        public PackType PackType {get; set;}
    }
}