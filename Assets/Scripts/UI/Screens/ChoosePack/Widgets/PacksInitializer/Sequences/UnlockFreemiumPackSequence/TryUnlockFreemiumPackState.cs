using System.Collections;
using System.Collections.Generic;
using Common.Currency;
using Extensions;
using Services;
using Services.Player;
using Utilities.StateMachine;
using Zenject;

namespace UI.Screens.ChoosePack.Widgets.PacksInitializer.Sequences.UnlockFreemiumPackSequence
{
    public class TryUnlockFreemiumPackState : InjectableStateBase<UnlockFreemiumPackSequenceContext>
    {
        
        [Inject] private readonly ProgressProvider _progressProvider;
        [Inject] private readonly PlayerCurrencyManager _playerCurrencyManager;
        [Inject] private readonly PlayerProfileController _playerProfileController;
        
        public override void OnEnter(params object[] arguments)
        {
            base.OnEnter(arguments);

            if (!CanBuyPack())
            {
                FinishSequence();
                return;
            }

            if (!_playerCurrencyManager.TrySpendCurrencies(Context.PackCost, out var newCurrencies, CurrencyChangeMode.Animated))
            {
                FinishSequence();
                return;
            }

            if (!_playerProfileController.CreateEmptyPack(Context.PackId, Context.PackType))
            {
                FinishSequence();
                return;
            }

            _playerProfileController.SaveProfile(SavePriority.ImmediateSave);
            NextState(newCurrencies);
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

            if (_playerProfileController.TryGetPackSnapshot(Context.PackId) != null)
            {
                LoggerService.LogWarning(this, $"Pack {Context.PackId} already exists in profile");
                return false;
            }

            return true;
        }

        private void NextState(List<ICurrency> newCurrencies)
        {
            Sequence.ActivateState<AnimateFreemiumPackUnlockingState>(newCurrencies);
        }

        private void FinishSequence()
        {
            LoggerService.LogWarning(this, $"Finish sequence without unlocking {Context.PackId}");
            Sequence.Finish();
        }
    }
}