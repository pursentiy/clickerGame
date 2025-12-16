using System.Collections.Generic;

namespace Storage.Snapshots
{
    public class PackSnapshot
    {
        public PackSnapshot(int packNumber, List<LevelSnapshot> completedLevelsSnapshots)
        {
            PackNumber = packNumber;
            CompletedLevelsSnapshots = completedLevelsSnapshots;
        }

        public int PackNumber {get; set;}
        public List<LevelSnapshot> CompletedLevelsSnapshots {get; set;}
    }
}