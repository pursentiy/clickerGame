using System;
using Common.Currency;
using Extensions;
using RSG;
using Services.FlyingRewardsAnimation;
using UI.Popups.DailyRewardPopup.DailyRewardDayItem.Animations;
using UnityEngine;
using UnityEngine.UI;
using Utilities;
using Utilities.Disposable;
using Zenject;

namespace UI.Popups.DailyRewardPopup.DailyRewardDayItem
{
    public class DailyRewardDayItem : MonoBehaviour, IDailyRewardAnimationContext
    {
        [Inject] private CurrencyLibraryService _currencyLibraryService;
        [Inject] private DailyRewardItemAnimator.Factory _animatorFactory;

        [Header("References")]
        [SerializeField] private RectTransform rootTransform;

        [Header("State views (one active per state)")]
        [SerializeField] private DailyRewardAlreadyCollectedView alreadyCollectedView;
        [SerializeField] private DailyRewardLockedView lockedView;
        [SerializeField] private DailyRewardAlreadyReadyToCollectView readyToCollectView;
        [SerializeField] private DailyRewardMissedDayView missedDayView;
        
        [Header("Animation References")]
        [SerializeField] private RectTransform contentHolder;
        [SerializeField] private Canvas itemCanvas;
        [SerializeField] private CanvasGroup itemCanvasGroup;
        [SerializeField] private float readyBounce = 0.08f;
        
        private int _dayIndex = 1;
        private Sprite _rewardIconSprite;
        private string _rewardAmountText = string.Empty;
        private DailyRewardItemAnimator _animator;
        private DayItemState _currentState;
        
        public RectTransform ContentHolder => contentHolder;
        public Canvas ItemCanvas => itemCanvas;
        public CanvasGroup ItemCanvasGroup => itemCanvasGroup;
        public IDisposeProvider DisposeProvider => gameObject.GetDisposeProvider();
        public float ReadyBounce => readyBounce;
        public int InitialSortingOrder { get; private set; }
        public Vector3 InitialScale { get; private set; }
        public Vector2 InitialPos { get; private set; }
        public RectTransform RootTransform => rootTransform;
        
        [Inject]
        public void Construct()
        {
            _animator = _animatorFactory.Create(this);
        }
        
        public void PlayRaysAnimation() => readyToCollectView?.PlayRaysAnimation();
        public void StopRaysAnimation() => readyToCollectView?.StopRayAnimation();
        public void PlayDust() => readyToCollectView?.PlayDust();

        public void InitializeItem(int dayIndex, DayItemState state, ICurrency rewardCurrency)
        {
            _dayIndex = dayIndex;
            _rewardIconSprite = _currencyLibraryService.GetMainIcon(rewardCurrency.GetType().Name);
            _rewardAmountText = rewardCurrency.GetCount().ToString();
            _currentState = state;

            // Заставляем Layout расставиться ПРЯМО СЕЙЧАС
            Canvas.ForceUpdateCanvases();
            if (transform.parent is RectTransform parentRT) 
                LayoutRebuilder.ForceRebuildLayoutImmediate(parentRT);

            SaveInitialParams();
            ApplyState(state);
        }

        public void PrepareForEntrance() => _animator.SetupInvisibleState();
        public IPromise PlayEntranceAnimation(float delay)
        {
            
            return _animator.PlayEntrance(delay)
                .Then(() => PlayIdleAnimationForState(_currentState))
                .CancelWith(this);
        }

        public IPromise PlayExitAnimation(float duration = 0.25f) => _animator.PlayExit(duration);
        public IPromise PlayClaimFeedbackAnimation(Action onImpact, Action onComplete)
        {
            return _animator.PlayClaim(() => onImpact?.SafeInvoke())
                .Then(() => onComplete?.SafeInvoke())
                .CancelWith(this);
        }
        
        public void SetState(DayItemState state, bool resetAnimation = true)
        {
            _currentState = state;
            
            if (resetAnimation)
                _animator.Reset();

            UpdateViewVisuals(state);
            PlayIdleAnimationForState(state);
        }
        
        private void OnDestroy() => _animator?.Dispose();
        
        private void ApplyState(DayItemState state)
        {
            _animator.Reset(); 
            
            UpdateViewVisuals(state);
            PlayIdleAnimationForState(state);
        }

        private void SaveInitialParams()
        {
            if (itemCanvas != null) InitialSortingOrder = itemCanvas.sortingOrder;
            if (contentHolder != null)
            {
                InitialScale = contentHolder.localScale;
                InitialPos = contentHolder.anchoredPosition;
            }
        }
        
        private void UpdateViewVisuals(DayItemState state)
        {
            SetViewActive(alreadyCollectedView, state == DayItemState.Collected);
            SetViewActive(lockedView, state == DayItemState.ToBeCollected);
            SetViewActive(readyToCollectView, state == DayItemState.ReadyToReceive);
            SetViewActive(missedDayView, state == DayItemState.MissedDay);

            var activeView = GetViewForState(state);
            if (activeView != null)
            {
                activeView.SetRewardIcon(_rewardIconSprite);
                activeView.SetRewardText(_rewardAmountText);
                activeView.SetInfoText(GetInfoText(state));
                activeView.ApplyVisuals(state);
            }
        }
        
        private void PlayIdleAnimationForState(DayItemState state)
        {
            switch (state)
            {
                case DayItemState.ReadyToReceive:
                    _animator.PlayReadyLoop();
                    break;
                case DayItemState.ToBeCollected:
                    _animator.PlayLockedSubtle();
                    break;
                case DayItemState.Collected:
                    _animator.PlayCollectedSubtle();
                    break;
                case DayItemState.MissedDay:
                    _animator.PlayMissedIdle();
                    break;
            }
        }

        private void SetViewActive(DailyRewardViewBase view, bool active)
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
                DayItemState.MissedDay => missedDayView,
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
                DayItemState.MissedDay => "Missed",
                _ => $"Day {_dayIndex}"
            };
        }
    }
}
