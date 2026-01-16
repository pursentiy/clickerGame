using Common.Currency;
using Plugins.FSignal;
using Zenject;

namespace Services
{
    public class PlayerCurrencyService
    {
        [Inject] private readonly PlayerProfileManager _playerProfileManager;
        
        public FSignal<Stars> StarsChangedSignal = new ();
        public Stars Stars => _playerProfileManager.ProfileSnapshot?.Stars ?? new Stars();
        
        public void AddStars(int amount)
        {
            if (_playerProfileManager == null)
            {
                LoggerService.LogError("ProfileSnapshot is null");
                return;
            }

            if (amount == 0)
            {
                return;
            }
            
            _playerProfileManager.ProfileSnapshot.Stars += amount;
            StarsChangedSignal.Dispatch(amount);
        }
    }
}