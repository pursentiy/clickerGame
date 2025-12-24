using System;
using System.Collections.Generic;

namespace Storage.Records
{
    [Serializable]
    public class ProfileRecord
    {
        public ProfileRecord(int stars, List<PackRecord> packRecords)
        {
            Stars = stars;
            PackRecords = packRecords;
        }

        public int Stars;
        public List<PackRecord> PackRecords;
    }
}