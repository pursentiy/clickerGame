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
        public List<LevelRecord> CompletedLevelsRecords;
        
        //TODO IN THE FUTURE BLOCK
        public bool IsNew;
        public UnlockStatus IsUnlocked;
    }
}