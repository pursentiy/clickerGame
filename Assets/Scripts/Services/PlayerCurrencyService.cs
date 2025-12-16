using Plugins.FSignal;
using Storage.Snapshots;
using UnityEngine;
using Zenject;

namespace Services
{
    public class PlayerCurrencyService
    {
        [Inject] private PlayerSnapshotService _playerSnapshotService;
        
        public FSignal<int> StarsChangedSignal = new FSignal<int>();
        public int Stars => _playerSnapshotService.ProfileSnapshot?.Stars ?? 0;
        
        public void AddStars(int amount)
        {
            if (_playerSnapshotService == null)
            {
                Debug.LogError("ProfileSnapshot is null");
                return;
            }
            
            _playerSnapshotService.ProfileSnapshot.Stars += amount;
            StarsChangedSignal.Dispatch( _playerSnapshotService.ProfileSnapshot.Stars);
        }
    }
}