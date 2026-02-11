using System.Collections.Generic;
using Attributes;
using Common.Currency;
using Configurations.DailyReward;
using Controllers;
using DG.Tweening;
using Extensions;
using Handlers.UISystem;
using RSG;
using Services;
using Services.ContentDeliveryService;
using Services.FlyingRewardsAnimation;
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
        [Inject] private readonly DailyRewardService _dailyRewardService;
        [Inject] private readonly FlowPopupController _flowPopupController;
        [Inject] private readonly AddressableContentDeliveryService _contentDeliveryService;
        [Inject] private readonly CurrencyLibraryService _currencyLibraryService;

        private List<GameObject> _spawnedRewardItems = new List<GameObject>();

        public override IUIPopupAnimation Animation => new ScalePopupAnimation(View.MainTransform);

        public override void OnCreated()
        {
            base.OnCreated();

            SetupTexts();
            SetupButtons();
            SetupDayRewards();
            SetupRewardPanels();
        }

        protected void OnDisposing()
        {
            CleanupRewardItems();
        }

        private void SetupTexts()
        {
            View.TitleText.text = "Daily Reward";

            View.DayText.text = $"Day {Context.DayIndex}";
        }

        private void SetupButtons()
        {
            View.ClaimRewardsButton.onClick.MapListenerWithSound(OnClaimRewardsClicked).DisposeWith(this);
            View.CloseButton.onClick.MapListenerWithSound(Hide).DisposeWith(this);
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

            var status = _dailyRewardService.GetRewardStatus();
            var currentDayIndex = Context.DayIndex;

            for (int day = 1; day <= DailyRewardConfiguration.CycleLength; day++)
            {
                var itemIndex = day - 1;
                if (itemIndex >= View.DayRewardItems.Length || View.DayRewardItems[itemIndex] == null)
                    continue;

                var item = View.DayRewardItems[itemIndex];
                
                // Collected: days before current day index
                var isCollected = day < currentDayIndex;
                // Current: the day being picked now
                var isCurrent = day == currentDayIndex;
                // Future: days after current day index
                var isFuture = day > currentDayIndex;

                SetupDayRewardItem(item, day, isCollected, isCurrent, isFuture);
            }
        }

        private void SetupDayRewardItem(DailyRewardDayItem item, int dayIndex, bool isCollected, bool isCurrent, bool isFuture)
        {
            if (item.RootTransform == null)
                return;

            // Set reward icon
            if (item.RewardIcon != null)
            {
                item.RewardIcon.gameObject.SetActive(true);
                
                // Get rewards for this day and set the icon
                if (Context.RewardsByDay != null && Context.RewardsByDay.TryGetValue(dayIndex, out var rewardsForDay) && rewardsForDay != null && rewardsForDay.Count > 0)
                {
                    // Use the first currency's icon (or combine if multiple)
                    var firstCurrency = rewardsForDay[0];
                    var currencyName = firstCurrency.GetType().Name;
                    var iconSprite = _currencyLibraryService.GetMainIcon(currencyName);
                    
                    if (iconSprite != null)
                    {
                        item.RewardIcon.sprite = iconSprite;
                    }
                }
                
                if (isCollected || isCurrent)
                {
                    // Show reward icon
                    item.RewardIcon.color = isCollected ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white;
                }
                else
                {
                    item.RewardIcon.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                }
            }

            // Set lock icon and text for future days
            item.LockIcon.gameObject.SetActive(isFuture);

            item.LockText.gameObject.SetActive(isFuture);
            if (isFuture)
            {
                item.LockText.text = "be unlocked soon";
            }

            // Set canvas group alpha
            item.CanvasGroup.alpha = isFuture ? 0.5f : 1f;

            // Animate current day (glowing and bouncing)
            if (isCurrent)
            {
                AnimateCurrentDayReward(item);
            }
            else
            {
                // Stop any animations
                if (item.RootTransform != null)
                {
                    item.RootTransform.DOKill();
                }
            }
        }

        private void AnimateCurrentDayReward(DailyRewardDayItem item)
        {
            if (item.RootTransform == null)
                return;

            item.RootTransform.DOKill();

            // Glow particles
            if (item.GlowParticles != null)
            {
                item.GlowParticles.Stop();
                item.GlowParticles.Play();
            }

            // Bouncing animation
            var originalScale = Vector3.one;
            var bounceSequence = DOTween.Sequence().KillWith(item.RootTransform.gameObject);
            
            bounceSequence.Append(item.RootTransform.DOScale(originalScale * 1.15f, 0.5f).SetEase(Ease.OutQuad));
            bounceSequence.Append(item.RootTransform.DOScale(originalScale, 0.5f).SetEase(Ease.InQuad));
            bounceSequence.SetLoops(-1, LoopType.Restart);

            // Shake animation
            item.RootTransform.DOShakePosition(1f, strength: 5f, vibrato: 10, randomness: 90, snapping: false, fadeOut: false)
                .SetLoops(-1, LoopType.Restart)
                .KillWith(item.RootTransform.gameObject);
        }

        private void SetupRewardPanels()
        {
            SetupCurrentRewardPanel();
            SetupNextRewardPanel();
        }

        private void SetupCurrentRewardPanel()
        {
            if (View.CurrentRewardContainer == null || Context.EarnedDailyReward == null)
                return;

            CleanupRewardItemsInContainer(View.CurrentRewardContainer);

            foreach (var currency in Context.EarnedDailyReward)
            {
                SpawnRewardItem(currency, View.CurrentRewardContainer);
            }
        }

        private void SetupNextRewardPanel()
        {
            if (View.NextRewardContainer == null || Context.RewardsByDay == null)
                return;

            CleanupRewardItemsInContainer(View.NextRewardContainer);

            var nextDayIndex = Context.DayIndex + 1;
            if (nextDayIndex > DailyRewardConfiguration.CycleLength)
                nextDayIndex = 1;

            if (Context.RewardsByDay.TryGetValue(nextDayIndex, out var nextRewards))
            {
                foreach (var currency in nextRewards)
                {
                    SpawnRewardItem(currency, View.NextRewardContainer);
                }
            }
        }

        private void SpawnRewardItem(ICurrency currency, Transform parent)
        {
            if (View.RewardItemPrefab == null || parent == null)
                return;

            _contentDeliveryService.InstantiateAsync(View.RewardItemPrefab, parent)
                .Then(disposableContent =>
                {
                    if (disposableContent?.Asset == null)
                        return;

                    _spawnedRewardItems.Add(disposableContent.Asset);
                    SetupRewardItemVisuals(disposableContent.Asset, currency);
                })
                .CancelWith(this);
        }

        private void SetupRewardItemVisuals(GameObject rewardItem, ICurrency currency)
        {
            // Try to find Image component for currency icon
            var image = rewardItem.GetComponentInChildren<Image>();
            if (image != null)
            {
                var sprite = _currencyLibraryService.GetMainIcon(currency.GetType().Name);
                if (sprite != null)
                {
                    image.sprite = sprite;
                }
            }

            // Try to find TextMeshProUGUI for amount display
            var text = rewardItem.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (text != null)
            {
                text.text = currency.GetCount().ToString();
            }
        }

        private void CleanupRewardItems()
        {
            foreach (var item in _spawnedRewardItems)
            {
                if (item != null)
                {
                    Destroy(item);
                }
            }
            _spawnedRewardItems.Clear();
        }

        private void CleanupRewardItemsInContainer(Transform container)
        {
            if (container == null)
                return;

            for (int i = container.childCount - 1; i >= 0; i--)
            {
                var child = container.GetChild(i);
                if (child != null)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private void OnClaimRewardsClicked()
        {
            if (View.ClaimRewardsButton != null)
            {
                View.ClaimRewardsButton.interactable = false;
            }

            PlayClaimAnimationSequence()
                .Then(() =>
                {
                    if (!_dailyRewardService.TryClaimTodayReward(out var claimedInfo))
                    {
                        Hide();
                        return;
                    }

                    SetupDayRewards();
                    Hide();
                })
                .CancelWith(this);
        }

        private IPromise PlayClaimAnimationSequence()
        {
            var promise = new Promise();

            if (View.CurrentRewardContainer == null)
            {
                promise.SafeResolve();
                return promise;
            }

            var seq = DOTween.Sequence().KillWith(this);

            // Animate current reward items
            var rewardItems = new List<RectTransform>();
            for (int i = 0; i < View.CurrentRewardContainer.childCount; i++)
            {
                var child = View.CurrentRewardContainer.GetChild(i);
                var rectTransform = child.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rewardItems.Add(rectTransform);
                }
            }

            if (rewardItems.Count > 0)
            {
                // Scale up animation for each reward item
                foreach (var item in rewardItems)
                {
                    seq.Join(item.DOPunchScale(Vector3.one * 0.3f, 0.5f, 10, 0.9f));
                }

                seq.AppendInterval(0.3f);

                // Fade out animation
                foreach (var item in rewardItems)
                {
                    var canvasGroup = item.GetComponent<CanvasGroup>();
                    if (canvasGroup == null)
                    {
                        canvasGroup = item.gameObject.AddComponent<CanvasGroup>();
                    }
                    seq.Join(canvasGroup.DOFade(0f, 0.4f));
                }
            }
            else
            {
                seq.AppendInterval(0.5f);
            }

            seq.OnComplete(() => promise.SafeResolve());

            return promise;
        }
    }
}

