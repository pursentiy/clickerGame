using System;
using System.Collections.Generic;
using Configurations.Progress;

namespace Storage.Records
{
    [Serializable]
    public class PackRecord
    {
        public PackRecord(int packNumber, List<LevelRecord> completedLevelsRecords, UnlockStatus isUnlocked, PackType packType = PackType.Default)
        {
            PackNumber = packNumber;
            CompletedLevelsRecords = completedLevelsRecords;
            IsUnlocked = isUnlocked;
            PackType = packType; // Defaults to PackType.Default (0) for existing DB values
        }
        
        public int PackNumber;
        public List<LevelRecord> CompletedLevelsRecords;
        
        //TODO IN THE FUTURE BLOCK
        public bool IsNew;
        public UnlockStatus IsUnlocked;
        public PackType PackType;
    }
}