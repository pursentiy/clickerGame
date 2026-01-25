using System;
using Components.UI;
using UnityEngine;
using Utilities.StateMachine;

namespace UI.Screens.ChoosePack.AdsSequence
{
    public class RewardedAdsSequenceContext : IStateContext
    {
        public RewardedAdsSequenceContext(
            CurrencyDisplayWidget currencyDisplayWidget,
            Action updatePacksAction,
            RectTransform adsButtonTransform,
            RectTransform adsRewardsVisualizationContainer)
        {
            CurrencyDisplayWidget = currencyDisplayWidget;
            UpdatePacksAction = updatePacksAction;
            AdsButtonTransform = adsButtonTransform;
            AdsRewardsVisualizationContainer = adsRewardsVisualizationContainer;
        }

        public CurrencyDisplayWidget CurrencyDisplayWidget { get; }
        public Action UpdatePacksAction { get; }
        public RectTransform AdsButtonTransform { get; }
        public RectTransform AdsRewardsVisualizationContainer { get; }
    }
}