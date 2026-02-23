using System;
using System.Collections.Generic;
using Common.Currency;
using Common.Data.Info;
using Components.UI;
using Configurations.Progress;
using Controllers;
using Extensions;
using RSG;
using Services;
using Services.Configuration;
using Services.FlyingRewardsAnimation;
using ThirdParty.SuperScrollView.Scripts.List;
using UI.Popups.MessagePopup;
using UI.Screens.ChoosePack.PackLevelItem.Base;
using UI.Screens.ChoosePack.PackLevelItem.DefaultPackItem;
using UI.Screens.ChoosePack.Widgets.PacksInitializer;
using UI.Screens.ChoosePack.Widgets.PacksInitializer.Sequences.NoCurrencySequence;
using UnityEngine;
using Utilities;
using Utilities.Disposable;
using Utilities.StateMachine;
using Zenject;

namespace UI.Screens.ChoosePack.Widgets
{
    public class DefaultPackInitializerWidget : BasePackInitializerWidget
    {
        [Inject] private readonly GameInfoProvider _gameInfoProvider;
        [Inject] private readonly LocalizationService _localizationService;
        [Inject] private readonly CurrencyLibraryService _currencyLibraryService;
        [Inject] private readonly FlowPopupController _flowPopupController;

        protected override PackType TargetPackType => PackType.Default;
        
        protected override void LaunchUnavailablePackSequenceOnClick(List<ICurrency> desiredCurrency, RectTransform popupAnchorRect)
        {
            StateMachine
                .CreateMachine(new VisualizeNotEnoughCurrencyContext(_currencyDisplayWidget, _adsButtonWidget, desiredCurrency, GetShowMessagePopupPromiseFunc(popupAnchorRect)))
                .StartSequence<VisualizeNotEnoughCurrencyState>()
                .FinishWith(this);
        }

        protected override Func<IDisposeProvider, IPromise<MediatorFlowInfo>> GetShowMessagePopupPromiseFunc(UnityEngine.RectTransform popupAnchorRect)
        {
            return (disposeProvider) =>
            {
                var currencyToEarnViaAds = _gameInfoProvider.StarsRewardForAds;
                var spriteAsset = _currencyLibraryService.GetSpriteAsset(CurrencyExtensions.StarsCurrencyName);
                var anchorRect = _adsButtonWidget != null ? _adsButtonWidget.RectTransform : popupAnchorRect;
                var context = new MessagePopupContext(_localizationService.GetFormattedValue(LocalizationExtensions.AdsInfo, currencyToEarnViaAds), anchorRect, MessagePopupFontSize, spriteAsset);
                var flowInfo = _flowPopupController.ShowMessagePopup(context, overrideDisposeProvider: disposeProvider);
                return Promise<MediatorFlowInfo>.Resolved(flowInfo);
            };
        }

        protected override IListItem CreateMediator(BasePackItemWidgetInfo info)
        {
            return new DefaultPackItemWidgetMediator((DefaultPackItemWidgetInfo)info);
        }

        protected override BasePackItemWidgetInfo CreatePackWidgetInfoInternal(PackInfo packInfo, int packId, bool isUnlocked, List<ICurrency> currencyToUnlock, int indexInList, System.Func<bool> getEntranceAnimationsAlreadyTriggered)
        {
            return new DefaultPackItemWidgetInfo(
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
