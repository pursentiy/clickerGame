using System.Collections.Generic;

namespace Storage.Snapshots
{
    public class ProfileSnapshot
    {
        public ProfileSnapshot(int stars, List<PackSnapshot> packSnapshots)
        {
            Stars = stars;
            PackSnapshots = packSnapshots;
        }

        public int Stars {get; set;}
        public List<PackSnapshot> PackSnapshots {get; set;}
        
    }
}