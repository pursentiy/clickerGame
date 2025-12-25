using Plugins.FSignal;
using Storage.Snapshots;
using UnityEngine;
using Zenject;

namespace Services
{
    public class PlayerCurrencyService
    {
        [Inject] private PlayerService _playerService;
        
        public FSignal<int> StarsChangedSignal = new FSignal<int>();
        public int Stars => _playerService.ProfileSnapshot?.Stars ?? 0;
        
        public void AddStars(int amount)
        {
            if (_playerService == null)
            {
                LoggerService.LogError("ProfileSnapshot is null");
                return;
            }

            if (amount == 0)
            {
                return;
            }
            
            _playerService.ProfileSnapshot.Stars += amount;
            StarsChangedSignal.Dispatch( _playerService.ProfileSnapshot.Stars);
        }
    }
}