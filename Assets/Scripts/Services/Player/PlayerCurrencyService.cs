using Common.Currency;
using Plugins.FSignal;
using Zenject;

namespace Services.Player
{
    public class PlayerCurrencyService
    {
        [Inject] private readonly PlayerProfileManager _playerProfileManager;
        
        public FSignal<Stars> StarsChangedSignal = new ();
        public Stars Stars => _playerProfileManager.Stars;
        
        public bool TryAddStars(int amount)
        {
            if (!_playerProfileManager.IsInitialized)
            {
                LoggerService.LogError(this,$"{nameof(TryAddStars)}: {nameof(PlayerProfileManager)} is not initialized.");
                return false;
            }

            if (amount <= 0)
            {
                return false;
            }
            
            _playerProfileManager.UpdateStarsAndSave(amount);
            StarsChangedSignal.Dispatch(Stars);
            return true;
        }
        
        public bool TrySpendStars(int amount)
        {
            if (!_playerProfileManager.IsInitialized)
            {
                LoggerService.LogError(this,$"{nameof(TryAddStars)}: {nameof(PlayerProfileManager)} is not initialized.");
                return false;
            }
            
            if (Stars.GetCount() < amount)
            {
                return false;
            }
    
            _playerProfileManager.UpdateStarsAndSave(-amount);
            StarsChangedSignal.Dispatch(Stars);
            return true;
        }
    }
}