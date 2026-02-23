using System.Collections.Generic;
using Common.Currency;
using Extensions;
using Services;
using Services.Player;
using Utilities.StateMachine;
using Zenject;

namespace UI.Screens.ChoosePack.Widgets.PacksInitializer.Sequences.UnlockFreemiumPackSequence
{
    public class TryUnlockFreemiumPackState: InjectableStateBase<UnlockFreemiumPackSequenceContext>
    {
        
        [Inject] private readonly ProgressProvider _progressProvider;
        [Inject] private readonly PlayerCurrencyManager _playerCurrencyManager;
        
        public override void OnEnter(params object[] arguments)
        {
            base.OnEnter(arguments);

            
        }
        
        private void RunBuyPackSequence(List<ICurrency> desiredCurrency, int packId)
        {
            foreach (var currency in desiredCurrency)
            {
                if (currency != null && currency.GetCount() > 0)
                    _playerCurrencyManager.TrySpendCurrency(currency);
            }
            UpdatePacksState();
        }

        private bool CanBuyPack()
        {
            if (!_progressProvider.GetPackStatus(Context.PackId).CanBeUnlocked())
            {
                LoggerService.LogWarning(this, $"Pack cannot be unlocked {Context.PackId}");
                return false;
            }

            if (!_progressProvider.IsPackAvailableForUnlocking(Context.PackId))
            {
                LoggerService.LogWarning(this, $"Not Enough currency for unlocking {Context.PackId}");
                return false;
            }

            return false;
        }
    }
}