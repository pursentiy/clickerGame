using Storage.Extensions;
using Storage.Snapshots;
using Zenject;

namespace Services
{
    public class PlayerRepositoryService
    {
        [Inject] private ProfileSerializerService _profileSerializerService;
        
        public bool HasProfile => LoadPlayerSnapshot() != null;
        
        public void SavePlayerSnapshot(ProfileSnapshot snapshot)
        {
            _profileSerializerService.SaveProfileRecord(snapshot.ToRecord());
        }

        public ProfileSnapshot LoadPlayerSnapshot()
        {
            var profileRecord = _profileSerializerService.LoadProfileRecord();

            return profileRecord?.ToSnapshot();
        }
    }
}