using Common.Currency;
using Plugins.FSignal;
using Zenject;

namespace Services.Player
{
    public class PlayerCurrencyManager
    {
        [Inject] private readonly PlayerProfileController _playerProfileController;
        
        public FSignal<ICurrency, CurrencyChangeMode> CurrencyChangedSignal = new ();
        
        public bool TryAddCurrency(ICurrency currency, CurrencyChangeMode mode = CurrencyChangeMode.Instant)
        {
            if (!_playerProfileController.IsInitialized)
            {
                LoggerService.LogError(this,$"{nameof(TryAddCurrency)}: {nameof(PlayerProfileController)} is not initialized.");
                return false;
            }

            if (currency.GetCount() <= 0)
            {
                return false;
            }

            if (!_playerProfileController.UpdateCurrencyAndSave(currency, out var currencyChange))
            {
                return false;
            }
            
            CurrencyChangedSignal.Dispatch(currencyChange, mode);
            return true;
        }
        
        public bool CanSpend(ICurrency amount)
        {
            if (!_playerProfileController.TryGetCurrency(amount.GetType(), out var current))
                return false;

            return current.GetCount() >= amount.GetCount();
        }
        
        public bool TrySpendCurrency(ICurrency amount, CurrencyChangeMode mode = CurrencyChangeMode.Instant)
        {
            if (!CanSpend(amount))
                return false;

            _playerProfileController.UpdateCurrencyAndSave(amount, out var newValue);

            CurrencyChangedSignal.Dispatch(newValue, mode);
            return true;
        }
    }
    
    public enum CurrencyChangeMode
    {
        Instant,
        Animated
    }
}