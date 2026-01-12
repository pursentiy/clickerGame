using Common.Currency;
using Plugins.FSignal;
using Zenject;

namespace Services
{
    public class PlayerCurrencyService
    {
        [Inject] private PlayerService _playerService;
        
        public FSignal<Stars> StarsChangedSignal = new ();
        public Stars Stars => _playerService.ProfileSnapshot?.Stars ?? new Stars();
        
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
            StarsChangedSignal.Dispatch(amount);
        }
    }
}