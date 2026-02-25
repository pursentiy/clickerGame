using Common.Currency;
using RSG;
using Services.FlyingRewardsAnimation;
using UnityEngine;
using Zenject;

namespace UI.Popups.DailyRewardPopup.DailyRewardDayItem
{
    public class DailyRewardDayItem : MonoBehaviour
    {
        [Inject] private CurrencyLibraryService _currencyLibraryService;

        [Header("References")]
        [SerializeField] private RectTransform rootTransform;
        [SerializeField] private DailyRewardItemAnimationWidget animationWidget;

        [Header("State views (one active per state)")]
        [SerializeField] private DailyRewardAlreadyCollectedView alreadyCollectedView;
        [SerializeField] private DailyRewardLockedView lockedView;
        [SerializeField] private DailyRewardAlreadyReadyToCollectView readyToCollectView;

        private int _dayIndex = 1;
        private Sprite _rewardIconSprite;
        private string _rewardAmountText = string.Empty;

        public RectTransform RootTransform => rootTransform;

        private void Awake()
        {
            if (animationWidget != null && readyToCollectView != null)
            {
                animationWidget.SetReadyToCollectCallbacks(
                    readyToCollectView.PlayGlow,
                    readyToCollectView.PlayDust,
                    readyToCollectView.StopGlow);
            }
        }

        public void InitializeItem(int dayIndex, DayItemState state, ICurrency rewardCurrency)
        {
            _dayIndex = dayIndex;
            if (rewardCurrency != null)
            {
                _rewardIconSprite = _currencyLibraryService.GetMainIcon(rewardCurrency.GetType().Name);
                _rewardAmountText = rewardCurrency.GetCount().ToString();
            }
            else
            {
                _rewardIconSprite = null;
                _rewardAmountText = string.Empty;
            }

            ApplyState(state);
        }
        
        public IPromise PlayEntranceAnimation(float delay, float duration = 0.45f)
        {
            return animationWidget != null ? animationWidget.PlayEntranceAnimation(delay, duration) : Promise.Resolved();
        }

        public IPromise PlayExitAnimation(float duration = 0.25f)
        {
            return animationWidget != null ? animationWidget.PlayExitAnimation(duration) : Promise.Resolved();
        }

        public IPromise PlayClaimFeedbackAnimation()
        {
            if (animationWidget == null)
                return Promise.Resolved();
            
            return animationWidget.PlayClaimFeedbackAnimation(() => UpdateState(DayItemState.Collected));
        }

        private void UpdateState(DayItemState state)
        {
            ApplyState(state);
        }

        private void ApplyState(DayItemState state)
        {
            if (animationWidget != null)
            {
                animationWidget.StopAnimations();
                animationWidget.ResetVisuals();
            }

            SetViewActive(alreadyCollectedView, state == DayItemState.Collected);
            SetViewActive(lockedView, state == DayItemState.ToBeCollected);
            SetViewActive(readyToCollectView, state == DayItemState.ReadyToReceive);

            DailyRewardViewBase activeView = GetViewForState(state);
            activeView.SetRewardIcon(_rewardIconSprite);
            activeView.SetRewardText(_rewardAmountText);
            activeView.SetInfoText(GetInfoText(state));
            activeView.ApplyVisuals(state);

            if (animationWidget == null)
                return;

            switch (state)
            {
                case DayItemState.ReadyToReceive:
                    animationWidget.PlayCurrentDayAnimation();
                    break;
                case DayItemState.ToBeCollected:
                    animationWidget.PlayLockedSubtleAnimation();
                    break;
            }
        }

        private static void SetViewActive(DailyRewardViewBase view, bool active)
        {
            view.gameObject.SetActive(active);
        }

        private DailyRewardViewBase GetViewForState(DayItemState state)
        {
            return state switch
            {
                DayItemState.Collected => alreadyCollectedView,
                DayItemState.ToBeCollected => lockedView,
                DayItemState.ReadyToReceive => readyToCollectView,
                _ => null
            };
        }

        private string GetInfoText(DayItemState state)
        {
            return state switch
            {
                DayItemState.Collected => "Collected",
                DayItemState.ReadyToReceive => "Ready to be collected!",
                DayItemState.ToBeCollected => $"Day {_dayIndex}",
                _ => $"Day {_dayIndex}"
            };
        }
    }
}
