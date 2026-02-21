using System.Collections.Generic;
using System.Linq;
using Attributes;
using Common.Currency;
using Configurations.DailyReward;
using Controllers;
using Extensions;
using Handlers.UISystem;
using RSG;
using Services;
using Services.CoroutineServices;
using Services.FlyingRewardsAnimation;
using Services.ScreenBlocker;
using TMPro;
using UI.Popups.CommonPopup;
using UI.Popups.MessagePopup;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Disposable;
using Zenject;

namespace UI.Popups.DailyRewardPopup
{
    [AssetKey("UI Popups/DailyRewardPopupMediator")]
    public class DailyRewardPopupMediator : UIPopupBase<DailyRewardPopupView, DailyRewardPopupContext>
    {
        private const float AwaitTimeAfterConsumingDailyRewards = 0.5f;

        private const string CollectRewardText = "Collect Reward";
        private const string ClosePopupText = "Close Popup";
        
        [Inject] private readonly DailyRewardService _dailyRewardService;
        [Inject] private readonly FlowPopupController _flowPopupController;
        [Inject] private readonly CurrencyLibraryService _currencyLibraryService;
        [Inject] private readonly FlyingUIRewardAnimationService _flyingUIRewardAnimationService;
        [Inject] private readonly CoroutineService _coroutineService;
        [Inject] private readonly UIGlobalBlocker _uiGlobalBlocker;

        private bool _canReceiveToday;
        private TMP_Text _primaryButtonText;

        public override IUIPopupAnimation Animation => new ScalePopupAnimation(View.MainTransform);

        public override void OnCreated()
        {
            base.OnCreated();

            SetupTexts();
            SetupButtons();
            RefreshAvailability();
            SetupDayRewards();
            UpdatePrimaryButtonUI();
        }

        private void SetupTexts()
        {
            View.TitleText.text = "Daily Reward";

            View.DayText.text = $"Day {Context.DayIndex}";
        }

        private void SetupButtons()
        {
            View.ClaimRewardsButton.onClick.MapListenerWithSound(OnPrimaryButtonClicked).DisposeWith(this);
            View.CloseButton.onClick.MapListenerWithSound(Hide).DisposeWith(this);
            View.BackgroundButton.onClick.MapListenerWithSound(Hide).DisposeWith(this);
            View.InfoButton.onClick.MapListenerWithSound(OnInfoClicked).DisposeWith(this);
        }

        private void OnInfoClicked()
        {
            var infoText = "Daily Rewards reset every day. Claim your reward each day to continue your streak! " +
                          "If you miss a day, your streak will reset to Day 1.";
            
            var context = new MessagePopupContext(
                infoText,
                View.MainTransform,
                fontSize: 80,
                spriteAsset: null,
                facing: PopupFacing.Left);
            
            _flowPopupController.ShowMessagePopup(context);
        }

        private void SetupDayRewards()
        {
            if (View.DayRewardItems == null || View.DayRewardItems.Length != DailyRewardConfiguration.CycleLength)
                return;

            var currentDayIndex = Context.DayIndex;
            var canReceiveToday = _canReceiveToday;

            for (int day = 1; day <= DailyRewardConfiguration.CycleLength; day++)
            {
                var itemIndex = day - 1;
                if (itemIndex >= View.DayRewardItems.Length || View.DayRewardItems[itemIndex] == null)
                    continue;

                var item = View.DayRewardItems[itemIndex];
                var state = day < currentDayIndex ? DayItemState.Collected
                    : day == currentDayIndex ? (canReceiveToday ? DayItemState.ReadyToReceive : DayItemState.ToBeCollected)
                    : DayItemState.ToBeCollected;

                SetupDayRewardItem(item, day, state);
            }
        }

        private void SetupDayRewardItem(DailyRewardDayItem item, int dayIndex, DayItemState state)
        {
            if (item == null)
                return;

            if (Context.RewardsByDay != null && Context.RewardsByDay.TryGetValue(dayIndex, out var rewardsForDay) && rewardsForDay != null && rewardsForDay.Count > 0)
            {
                var firstCurrency = rewardsForDay[0];
                var currencyName = firstCurrency.GetType().Name;
                var iconSprite = _currencyLibraryService.GetMainIcon(currencyName);
                item.SetRewardIcon(iconSprite);
            }

            item.SetupState(state);
        }

        private void OnPrimaryButtonClicked()
        {
            if (!_canReceiveToday)
            {
                Hide();
                return;
            }

            OnClaimRewardsClicked();
        }

        private void RefreshAvailability()
        {
            _canReceiveToday = _dailyRewardService.TryGetTodayRewardPreview(out var preview) &&
                               preview.DayIndex == Context.DayIndex;
        }

        private void UpdatePrimaryButtonUI()
        {
            View.ClaimRewardsButton.interactable = true;
            View.ClaimRewardsButtonText.text = _canReceiveToday ? CollectRewardText : ClosePopupText;
        }

        private void OnClaimRewardsClicked()
        {
            View.ClaimRewardsButton.interactable = false;
            
            var blockRef = _uiGlobalBlocker.Block(30f);
            PlayClaimAnimationSequence()
                .Then(() => VisualizeRewardsFlight(Context.EarnedDailyReward))
                .Then(() => View.CurrencyDisplayWidget.SetCurrency(Context.EarnedDailyReward.First(), true))
                .Then(() =>
                {
                    blockRef.Dispose();
                    RefreshAvailability();
                    UpdatePrimaryButtonUI();
                })
                .Catch(e =>
                {
                    LoggerService.LogError($"Failed to claim rewards for day {Context.DayIndex} with exception: {e}");
                    blockRef.Dispose();
                    Hide();
                })
                .CancelWith(this);
        }

        private IPromise PlayClaimAnimationSequence()
        {
            // Animate the current day's item (use DayRewardItems only, no spawning)
            var currentItemIndex = Context.DayIndex - 1;
            if (View.DayRewardItems != null && currentItemIndex >= 0 && currentItemIndex < View.DayRewardItems.Length)
            {
                var currentItem = View.DayRewardItems[currentItemIndex];
                if (currentItem != null)
                    return currentItem.PlayClaimFeedbackAnimation();
            }

            return Promise.Resolved();
        }
        
        private IPromise VisualizeRewardsFlight(IList<ICurrency> rewards)
        {
            if (rewards == null || rewards.Count == 0)
                return Promise.Resolved();

            if (View.FlyingRewardsContainer == null)
                return Promise.Resolved();

            if (View.CurrencyDisplayWidget == null || View.CurrencyDisplayWidget.AnimationTarget == null)
                return Promise.Resolved();

            Vector3 startPosition = GetRewardFlightStartPosition();
            Vector3 targetPosition = View.CurrencyDisplayWidget.AnimationTarget.position;

            var context = new FlyingUIRewardAnimationContext(
                rewards.ToArray(),
                View.FlyingRewardsContainer,
                new[] { startPosition },
                new[] { targetPosition });

            return _flyingUIRewardAnimationService.PlayAnimation(context).CancelWith(this);
        }

        private Vector3 GetRewardFlightStartPosition()
        {
            var currentItemIndex = Context.DayIndex - 1;
            if (View.DayRewardItems != null && currentItemIndex >= 0 && currentItemIndex < View.DayRewardItems.Length)
            {
                var currentItem = View.DayRewardItems[currentItemIndex];
                if (currentItem != null && currentItem.RootTransform != null)
                    return currentItem.RootTransform.position;
            }

            return View.FlyingRewardsContainer != null ? View.FlyingRewardsContainer.position : Vector3.zero;
        }

        /// <summary>
        /// Plays only the claim/receiving animation for the current day (no actual claim, no hide). Used for cheats and testing.
        /// </summary>
        public void PlayClaimReceivingAnimation()
        {
            PlayClaimAnimationSequence().CancelWith(this);
        }
    }
}