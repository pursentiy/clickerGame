using Common.Currency;
using DG.Tweening;
using Extensions;
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
        [SerializeField] private RectTransform contentHolder;
        [SerializeField] private Canvas itemCanvas;

        [Header("State views (one active per state)")]
        [SerializeField] private DailyRewardAlreadyCollectedView alreadyCollectedView;
        [SerializeField] private DailyRewardLockedView lockedView;
        [SerializeField] private DailyRewardAlreadyReadyToCollectView readyToCollectView;

        [Header("Animation Settings")]
        [SerializeField] private float _readyBounce = 0.1f;

        private int _initialSortingOrder;
        private Vector3 _initialContentScale = Vector3.one;
        private int _dayIndex = 1;
        private Sprite _rewardIconSprite;
        private string _rewardAmountText = string.Empty;

        public RectTransform RootTransform => rootTransform;

        private void Awake()
        {
            _initialSortingOrder = itemCanvas.sortingOrder;
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

        public void UpdateState(DayItemState state)
        {
            ApplyState(state);
        }

        private void ApplyState(DayItemState state)
        {
            StopAnimations();
            ResetVisuals();

            SetViewActive(alreadyCollectedView, state == DayItemState.Collected);
            SetViewActive(lockedView, state == DayItemState.ToBeCollected);
            SetViewActive(readyToCollectView, state == DayItemState.ReadyToReceive);

            DailyRewardViewBase activeView = GetViewForState(state);
            activeView.SetRewardIcon(_rewardIconSprite);
            activeView.SetRewardText(_rewardAmountText);
            activeView.SetInfoText(GetInfoText(state));
            activeView.ApplyVisuals(state);

            switch (state)
            {
                case DayItemState.ReadyToReceive:
                    PlayCurrentDayAnimation();
                    break;
                case DayItemState.ToBeCollected:
                    PlayLockedSubtleAnimation();
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

        private void ResetVisuals()
        {
            contentHolder.DOKill();
            contentHolder.localScale = _initialContentScale;
            contentHolder.localRotation = Quaternion.identity;
            contentHolder.anchoredPosition = Vector2.zero;

            itemCanvas.overrideSorting = false;
            itemCanvas.sortingOrder = _initialSortingOrder;
        }

        public void PlayCurrentDayAnimation()
        {
            StopAnimations();
            contentHolder.DOScale(_initialContentScale * (1f + _readyBounce), 1.2f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetId(this);

            contentHolder.DORotate(new Vector3(0, 0, 3f), 1.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetId(this);

            readyToCollectView.PlayGlow();
        }

        private void PlayLockedSubtleAnimation()
        {
            contentHolder.DOPunchPosition(new Vector3(2f, 0, 0), 2f, 1, 0.5f)
                .SetEase(Ease.Linear)
                .SetLoops(-1)
                .SetId(this);
        }

        public IPromise PlayClaimFeedbackAnimation()
        {
            var promise = new Promise();
            StopAnimations();

            itemCanvas.overrideSorting = true;
            itemCanvas.sortingOrder = _initialSortingOrder + 100;

            var seq = DOTween.Sequence().SetId(this);

            seq.Append(contentHolder.DOScale(new Vector3(1.2f, 0.7f, 1f), 0.1f).SetEase(Ease.OutQuad));
            seq.Append(contentHolder.DOScale(new Vector3(0.8f, 1.4f, 1f), 0.15f).SetEase(Ease.OutBack));
            seq.Join(contentHolder.DOLocalMoveY(40f, 0.15f).SetRelative().SetEase(Ease.OutCubic));
            seq.Append(contentHolder.DOScale(new Vector3(1.1f, 1.1f, 1f), 0.1f).SetEase(Ease.InBack));
            seq.Join(contentHolder.DOLocalMoveY(-40f, 0.1f).SetRelative().SetEase(Ease.InCubic));
            seq.AppendCallback(() =>
            {
                if (readyToCollectView != null)
                    readyToCollectView.PlayDust();
                contentHolder.DOShakeRotation(0.3f, 15f, 30);
                UpdateState(DayItemState.Collected);
            });
            seq.Append(contentHolder.DOScale(1f, 0.2f).SetEase(Ease.OutElastic));

            seq.OnComplete(() => promise.SafeResolve());

            return promise;
        }

        public void StopAnimations()
        {
            DOTween.Kill(this);
            contentHolder.DOKill();
            readyToCollectView.StopGlow();
        }

        private void OnDisable() => StopAnimations();
    }
}
