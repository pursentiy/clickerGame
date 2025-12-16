using System.Collections.Generic;

namespace Storage.Records
{
    public class PackRecord
    {
        public PackRecord(int packNumber, List<LevelRecord> completedLevelsRecords)
        {
            PackNumber = packNumber;
            CompletedLevelsRecords = completedLevelsRecords;
        }

        public int PackNumber {get; set;}
        public List<LevelRecord> CompletedLevelsRecords {get; set;}
    }
}