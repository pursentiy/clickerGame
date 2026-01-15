using System;
using Extensions;
using RSG;
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

        public IPromise<ProfileSnapshot> LoadPlayerSnapshot()
        {
            var snapshotPromise = new Promise<ProfileSnapshot>();
            _profileStorageService.LoadProfileRecord(OnLoaded);

            return snapshotPromise;
            void OnLoaded(ProfileRecord rawProfileRecord)
            {
                snapshotPromise.SafeResolve(rawProfileRecord?.ToSnapshot());
            }
        }
    }
}