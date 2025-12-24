using System;
using System.Collections.Generic;

namespace Storage.Records
{
    [Serializable]
    public class PackRecord
    {
        public PackRecord(int packNumber, List<LevelRecord> completedLevelsRecords)
        {
            PackNumber = packNumber;
            CompletedLevelsRecords = completedLevelsRecords;
        }

        public int PackNumber;
        public List<LevelRecord> CompletedLevelsRecords;
    }
}