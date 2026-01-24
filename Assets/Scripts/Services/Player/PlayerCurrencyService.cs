using Common.Currency;
using Plugins.FSignal;
using Zenject;

namespace Services.Player
{
    public class PlayerCurrencyService
    {
        [Inject] private readonly PlayerProfileManager _playerProfileManager;
        
        public FSignal<ICurrency, CurrencyChangeMode> StarsChangedSignal = new ();
        public Stars Stars => _playerProfileManager.Stars;
        
        public bool TryAddStars(Stars amount, CurrencyChangeMode mode = CurrencyChangeMode.Instant)
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
            StarsChangedSignal.Dispatch(Stars, mode);
            return true;
        }
        
        public bool TrySpendStars(Stars amount, CurrencyChangeMode mode = CurrencyChangeMode.Instant)
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
            StarsChangedSignal.Dispatch(Stars, mode);
            return true;
        }
    }
    
    public enum CurrencyChangeMode
    {
        Instant,
        Animated
    }
}