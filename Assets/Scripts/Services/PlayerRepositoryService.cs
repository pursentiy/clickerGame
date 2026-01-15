using System;
using Extensions;
using Storage.Extensions;
using Storage.Records;
using Storage.Snapshots;
using Zenject;

namespace Services
{
    public class PlayerRepositoryService
    {
        [Inject] private ProfileStorageService _profileStorageService;
        
        public void SavePlayerSnapshot(ProfileSnapshot snapshot)
        {
            _profileStorageService.SaveProfileRecord(snapshot.ToRecord());
        }

        public void LoadPlayerSnapshot(Action<ProfileSnapshot> onLoaded)
        {
            _profileStorageService.LoadProfileRecord(OnLoaded);

            void OnLoaded(ProfileRecord rawProfileRecord)
            {
                onLoaded.SafeInvoke(rawProfileRecord?.ToSnapshot());
            }
        }
    }
}