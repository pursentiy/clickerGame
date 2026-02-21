using System;
using System.Collections.Generic;
using Common.Currency;
using Common.Data.Info;
using Configurations.Progress;
using Controllers;
using Extensions;
using RSG;
using Services;
using Services.Configuration;
using Services.FlyingRewardsAnimation;
using Services.Player;
using ThirdParty.SuperScrollView.Scripts.List;
using UI.Popups.MessagePopup;
using UI.Screens.ChoosePack.PackLevelItem.Base;
using UI.Screens.ChoosePack.PackLevelItem.FreemiumPackItem;
using Utilities;
using Zenject;

namespace UI.Screens.ChoosePack.Widgets
{
    public class FreemiumPackInitializerWidget : BasePackInitializerWidget
    {
        [Inject] private readonly GameInfoProvider _gameInfoProvider;
        [Inject] private readonly LocalizationService _localizationService;
        [Inject] private readonly CurrencyLibraryService _currencyLibraryService;
        [Inject] private readonly FlowPopupController _flowPopupController;
        [Inject] private readonly PlayerCurrencyManager _playerCurrencyManager;

        protected override PackType TargetPackType => PackType.Freemium;

        protected override Func<IDisposeProvider, IPromise<MediatorFlowInfo>> GetShowMessagePopupPromiseFunc(UnityEngine.RectTransform popupAnchorRect)
        {
            return (disposeProvider) =>
            {
                var currencyToEarnViaAds = _gameInfoProvider.StarsRewardForAds;
                var spriteAsset = _currencyLibraryService.GetSpriteAsset(CurrencyExtensions.StarsCurrencyName);
                var context = new MessagePopupContext(_localizationService.GetFormattedValue(LocalizationExtensions.AdsInfo, currencyToEarnViaAds), popupAnchorRect, MessagePopupFontSize, spriteAsset);
                var flowInfo = _flowPopupController.ShowMessagePopup(context, overrideDisposeProvider: disposeProvider);
                return Promise<MediatorFlowInfo>.Resolved(flowInfo);
            };
        }

        protected override IListItem CreateMediator(BasePackItemWidgetInfo info)
        {
            return new FreemiumPackItemWidgetMediator((FreemiumPackItemWidgetInfo)info);
        }

        protected override bool TryStartBuySequenceIfAffordable(List<ICurrency> desiredCurrency, UnityEngine.RectTransform popupAnchorRect, int packId)
        {
            if (_progressProvider.EnoughCurrencyToUnlockPack(packId) != PackUnlockCurrencyStatus.AvailableToUnlock)
                return false;

            RunBuyPackSequence(desiredCurrency, packId);
            return true;
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

        protected override BasePackItemWidgetInfo CreatePackWidgetInfoInternal(PackInfo packInfo, int packId, bool isUnlocked, List<ICurrency> currencyToUnlock, int indexInList, System.Func<bool> getEntranceAnimationsAlreadyTriggered)
        {
            return new FreemiumPackItemWidgetInfo(
                packInfo.PackName,
                packInfo.PackImagePrefab,
                packId,
                isUnlocked,
                () => OnAvailablePackClicked(packInfo),
                OnUnavailablePackClicked,
                currencyToUnlock,
                indexInList,
                getEntranceAnimationsAlreadyTriggered);
        }
    }
}
