using System;
using System.Collections.Generic;

namespace Storage.Records
{
    [Serializable]
    public class PackRecord
    {
        public PackRecord(int packNumber, List<LevelRecord> completedLevelsRecords, UnlockStatus isUnlocked)
        {
            PackNumber = packNumber;
            CompletedLevelsRecords = completedLevelsRecords;
            IsUnlocked = isUnlocked;
        }
        
        public int PackNumber;
        public UnlockStatus IsUnlocked;
        public List<LevelRecord> CompletedLevelsRecords;
        public bool IsNew;
    }
}