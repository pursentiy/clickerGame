using System.Collections.Generic;
using Storage.Snapshots;

namespace Services
{
    public class ProfileBuilderService
    {
        public ProfileSnapshot BuildNewProfileSnapshot()
        {
            return new ProfileSnapshot(
                0,
                new List<PackSnapshot>());
        }
    }
}