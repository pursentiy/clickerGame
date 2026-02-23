using System.Collections.Generic;
using System.Linq;
using Common.Currency;
using Extensions;
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
            return amount is not null && TrySpendCurrencies(new List<ICurrency> { amount }, mode);
        }
        
        public bool TrySpendCurrencies(List<ICurrency> currencies, CurrencyChangeMode mode = CurrencyChangeMode.Instant)
        {
            if (currencies is not { Count: > 0 }) 
                return false;
            
            if (!currencies.All(c => c is not null && CanSpend(c)))
                return false;

            var spendDeltas = currencies
                .Select(c => c.SetCount(-c.GetCount()))
                .ToList();

            if (!_playerProfileController.UpdateCurrencyAndSave(spendDeltas, out var newValues))
                return false;

            foreach (var newValue in newValues)
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