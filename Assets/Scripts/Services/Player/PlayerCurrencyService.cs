using Common.Currency;
using Plugins.FSignal;
using Zenject;

namespace Services.Player
{
    public class PlayerCurrencyService
    {
        [Inject] private readonly PlayerProfileManager _playerProfileManager;
        
        public FSignal<ICurrency, CurrencyChangeMode> CurrencyChangedSignal = new ();
        public Stars Stars => _playerProfileManager.Stars;
        public SoftCurrency SoftCurrency => _playerProfileManager.SoftCurrency;
        
        public bool TryAddCurrency(ICurrency currency, CurrencyChangeMode mode = CurrencyChangeMode.Instant)
        {
            if (!_playerProfileManager.IsInitialized)
            {
                LoggerService.LogError(this,$"{nameof(TryAddCurrency)}: {nameof(PlayerProfileManager)} is not initialized.");
                return false;
            }

            if (currency.GetCount() <= 0)
            {
                return false;
            }
            
            _playerProfileManager.UpdateCurrencyAndSave(currency);
            CurrencyChangedSignal.Dispatch(_playerProfileManager.GetCurrencyCount(currency), mode);
            return true;
        }
        
        public bool TrySpendCurrency(ICurrency amount, CurrencyChangeMode mode = CurrencyChangeMode.Instant)
        {
            if (!_playerProfileManager.IsInitialized)
            {
                LoggerService.LogError(this,$"{nameof(TryAddCurrency)}: {nameof(PlayerProfileManager)} is not initialized.");
                return false;
            }
            
            if (!_playerProfileManager.CanSpendCurrency(amount))
            {
                return false;
            }
    
            _playerProfileManager.UpdateCurrencyAndSave(amount.Multiply(-1));
            CurrencyChangedSignal.Dispatch(_playerProfileManager.GetCurrencyCount(amount), mode);
            return true;
        }
    }
    
    public enum CurrencyChangeMode
    {
        Instant,
        Animated
    }
}