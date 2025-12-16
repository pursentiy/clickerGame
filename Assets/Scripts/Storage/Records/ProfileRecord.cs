using System.Collections.Generic;

namespace Storage.Records
{
    public class ProfileRecord
    {
        public ProfileRecord(int stars, List<PackRecord> packRecords)
        {
            Stars = stars;
            PackRecords = packRecords;
        }

        public int Stars {get; set;}
        public List<PackRecord> PackRecords {get; set;}
    }
}