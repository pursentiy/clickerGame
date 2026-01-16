using System.Collections.Generic;
using System.Linq;
using Common.Currency;
using Extensions;
using Plugins.FSignal;
using RSG;
using Storage.Snapshots;
using Zenject;

namespace Services.Player
{
    public class PlayerProfileManager
    {
        [Inject] private readonly PlayerRepositoryService _playerRepositoryService;
        
        public bool IsInitialized = false;
        public readonly FSignal ProfileSnapshotInitializedSignal = new ();
        
        
        public Stars Stars => _profileSnapshot?.Stars ?? new Stars(0);
        public IReadOnlyList<PackSnapshot> PacksSnapshot => _profileSnapshot?.PackSnapshots;
        
        
        public GameParamsSnapshot TryGetGameParamsSnapshot() => _profileSnapshot?.GameParamsSnapshot;
        
        
        private ProfileSnapshot _profileSnapshot { get; set; }

        public void UpdateStarsAndSave(int amount)
        {
            if (_profileSnapshot == null)
                return;
            
            _profileSnapshot.Stars += amount;
            SaveProfile();
        }
        
        public PackSnapshot TryGetPackSnapshot(int packNumber)
        {
            return _profileSnapshot?.PackSnapshots.FirstOrDefault(p => p.PackNumber == packNumber);
        }
        
        private void TryAddPackSnapshot(PackSnapshot packSnapshot)
        {
            if (_profileSnapshot == null)
                return;
            
            if (packSnapshot == null)
            {
                LoggerService.LogWarning(this, $"{nameof(PackSnapshot)} is null at {nameof(TryAddPackSnapshot)}");
                return;
            }

            if (_profileSnapshot.PackSnapshots.Any(i => i.PackNumber == packSnapshot.PackNumber))
            {
                LoggerService.LogWarning($"Already contains {nameof(packSnapshot.PackNumber)} at {nameof(TryAddPackSnapshot)}");
                return;
            }
            
            _profileSnapshot.PackSnapshots.Add(packSnapshot);
        }

        public void Initialize(ProfileSnapshot profileSnapshot)
        {
            if (profileSnapshot == null)
            {
                LoggerService.LogError($"{GetType().Name}.{nameof(Initialize)}: ProfileSnapshot is null");
            }
            
            _profileSnapshot = profileSnapshot;
            IsInitialized = true;
            ProfileSnapshotInitializedSignal.Dispatch();
        }
        
        public void SaveProfile()
        {
            if (_profileSnapshot == null)
            {
                LoggerService.LogError(this, $"{nameof(SaveProfile)}: Profile snapshot is null.");
                return;
            }
            
            //TODO ADD PROMISE AWAIT
            _playerRepositoryService.SavePlayerSnapshot(_profileSnapshot);
        }
        
        public IPromise<ProfileSnapshot> LoadProfile()
        {
            return _playerRepositoryService.LoadPlayerSnapshot();
        }
    }
}