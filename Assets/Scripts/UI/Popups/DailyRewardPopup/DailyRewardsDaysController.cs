using System;
using System.Collections.Generic;
using System.Linq;
using Common.Currency;
using Configurations.DailyReward;
using Extensions;
using RSG;
using Services;
using Services.DailyReward;
using Services.FlyingRewardsAnimation;
using Services.ScreenBlocker;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Disposable;
using Zenject;
using Components.UI;
using UI.Popups.DailyRewardPopup.DailyRewardDayItem;

namespace UI.Popups.DailyRewardPopup
{
    public class DailyRewardsDaysController : MonoBehaviour
    {
        private const string CollectRewardText = "Collect Reward";
        private const string ClosePopupText = "Close Popup";

        [Inject] private FlyingUIRewardAnimationService _flyingUIRewardAnimationService;
        [Inject] private DailyRewardsInfoProvider _dailyRewardsInfoProvider;
        [Inject] private UIGlobalBlocker _uiGlobalBlocker;

        [Header("Claim")]
        [SerializeField] private Button claimRewardsButton;
        [SerializeField] private TMP_Text claimRewardsButtonText;
        [SerializeField] private RectTransform flyingRewardsContainer;
        [SerializeField] private CurrencyDisplayWidget currencyDisplayWidget;

        private DailyRewardDayItem.DailyRewardDayItem[] _items;
        private DailyRewardPopupContext _context;
        private Action _hideAction;
        private bool _canReceiveToday;

        public bool CanReceiveToday => _canReceiveToday;

        private void Start()
        {
            claimRewardsButton.onClick.MapListenerWithSound(OnPrimaryButtonClicked).DisposeWith(this);
        }

        private void OnPrimaryButtonClicked()
        {
            if (!_canReceiveToday)
            {
                _hideAction?.SafeInvoke();
                return;
            }
            
            RunClaimFlow().CancelWith(this);
        }

        public void Initialize(DailyRewardDayItem.DailyRewardDayItem[] items, DailyRewardPopupContext context, Action hideAction)
        {
            _items = items;
            _context = context;
            _hideAction = hideAction;

            RefreshAvailability();

            if (_items == null || _items.Length != DailyRewardConfiguration.CycleLength)
                return;

            var currentDayIndex = _context.DayIndex;

            for (int day = 1; day <= DailyRewardConfiguration.CycleLength; day++)
            {
                var itemIndex = day - 1;
                if (itemIndex >= _items.Length || _items[itemIndex] == null)
                    continue;

                var item = _items[itemIndex];
                var state = day < currentDayIndex ? DayItemState.Collected
                    : day == currentDayIndex ? (_canReceiveToday ? DayItemState.ReadyToReceive : DayItemState.ToBeCollected)
                    : DayItemState.ToBeCollected;

                SetupDayRewardItem(item, day, state);
            }

            UpdatePrimaryButtonUI();
        }

        private void RefreshAvailability()
        {
            if (_context == null)
                return;
            
            _canReceiveToday = _dailyRewardsInfoProvider.TryGetTodayRewardPreview(out var preview) &&
                              preview.DayIndex == _context.DayIndex;
        }

        private void UpdatePrimaryButtonUI()
        {
            claimRewardsButton.interactable = true;
            claimRewardsButtonText.text = _canReceiveToday ? CollectRewardText : ClosePopupText;
        }

        private IPromise RunClaimFlow()
        {
            if (_context == null)
                return Promise.Resolved();

            claimRewardsButton.interactable = false;

            var blockRef = _uiGlobalBlocker.Block(30f);
            return PlayClaimAnimationSequence()
                .Then(() => VisualizeRewardsFlight(_context.EarnedDailyReward, flyingRewardsContainer, currencyDisplayWidget))
                .Then(() =>
                {
                    if (currencyDisplayWidget != null && _context?.EarnedDailyReward is { Count: > 0 })
                        currencyDisplayWidget.SetCurrency(_context.EarnedDailyReward.First(), true);
                })
                .Then(() =>
                {
                    blockRef.Dispose();
                    RefreshAvailability();
                    UpdatePrimaryButtonUI();
                })
                .Catch(e =>
                {
                    LoggerService.LogError($"Failed to claim rewards for day {_context?.DayIndex} with exception: {e}");
                    blockRef.Dispose();
                    _hideAction?.Invoke();
                });
        }

        public void PlayClaimReceivingAnimation()
        {
            PlayClaimAnimationSequence().CancelWith(this);
        }

        public DailyRewardDayItem.DailyRewardDayItem GetCurrentItem()
        {
            if (_context == null || _items == null)
                return null;

            var currentItemIndex = _context.DayIndex - 1;
            if (currentItemIndex < 0 || currentItemIndex >= _items.Length)
                return null;

            return _items[currentItemIndex];
        }

        public IPromise PlayClaimAnimationSequence()
        {
            var currentItem = GetCurrentItem();
            if (currentItem != null)
                return currentItem.PlayClaimFeedbackAnimation();

            return Promise.Resolved();
        }

        public IPromise VisualizeRewardsFlight(IList<ICurrency> rewards, UnityEngine.RectTransform flyingRewardsContainer, CurrencyDisplayWidget currencyDisplayWidget)
        {
            if (rewards == null || rewards.Count == 0)
                return Promise.Resolved();

            if (flyingRewardsContainer == null || currencyDisplayWidget == null)
                return Promise.Resolved();

            Vector3 startPosition = GetRewardFlightStartPosition(flyingRewardsContainer);
            var targetPositions = rewards.Select(r => currencyDisplayWidget.GetAnimationTarget(r)).ToArray();

            var context = new FlyingUIRewardAnimationContext(
                rewards.ToArray(),
                flyingRewardsContainer,
                new[] { startPosition },
                targetPositions);

            return _flyingUIRewardAnimationService.PlayAnimation(context);
        }

        public Vector3 GetRewardFlightStartPosition(UnityEngine.RectTransform fallbackContainer)
        {
            var currentItem = GetCurrentItem();
            if (currentItem != null && currentItem.RootTransform != null)
                return currentItem.RootTransform.position;

            return fallbackContainer != null ? fallbackContainer.position : Vector3.zero;
        }

        private void SetupDayRewardItem(DailyRewardDayItem.DailyRewardDayItem item, int dayIndex, DayItemState state)
        {
            ICurrency rewardCurrency = GetRewardCurrencyForDay(dayIndex);
            item.InitializeItem(dayIndex, state, rewardCurrency);
        }

        private ICurrency GetRewardCurrencyForDay(int dayIndex)
        {
            if (_context?.RewardsByDay == null || !_context.RewardsByDay.TryGetValue(dayIndex, out var rewardsForDay) || rewardsForDay == null || rewardsForDay.Count == 0)
                return null;

            return rewardsForDay[0];
        }
    }
}
