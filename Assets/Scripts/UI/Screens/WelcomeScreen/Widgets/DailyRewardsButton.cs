using Components.UI;
using Controllers;
using DG.Tweening;
using Extensions;
using Services;
using UI.Popups.DailyRewardPopup;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Disposable;
using Zenject;

namespace UI.Screens.WelcomeScreen.Widgets
{
    /// <summary>
    /// Daily rewards button widget that displays timer and handles daily reward popup opening.
    /// </summary>
    public class DailyRewardsButton : ButtonWithTimerBase
    {
        [Inject] private readonly DailyRewardService _dailyRewardService;
        [Inject] private readonly FlowPopupController _flowPopupController;

        [Header("Button")]
        [SerializeField] private Button Button;

        [Header("Animation")]
        [SerializeField] private RectTransform ButtonTransform;
        [SerializeField] private ParticleSystem GlowParticles;
        [SerializeField] private float ShakeStrength = 5f;
        [SerializeField] private int ShakeVibrato = 10;
        [SerializeField] private float ShakeRandomness = 90f;

        public void Initialize()
        {
            SetupButton();
            UpdateTimer();
        }

        public void UpdateTimer()
        {
            // Stop any running timer first
            StopTimer();

            var status = _dailyRewardService.GetRewardStatus();

            if (status.IsAvailable && _dailyRewardService.TryGetTodayRewardPreview(out _))
            {
                // Reward is available - show available text and start animations
                // No timer needed in this case
                if (TimerText != null)
                {
                    TimerText.text = AvailableText;
                }
                StartAnimations();
            }
            else
            {
                // Start countdown timer
                StopAnimations();
                
                // Only start timer if there's time remaining
                if (status.TimeUntilNext.TotalSeconds > 0)
                {
                    StartTimer(status.TimeUntilNext, OnTimerComplete);
                }
                else
                {
                    // Edge case: time is zero or negative, treat as available
                    if (TimerText != null)
                    {
                        TimerText.text = AvailableText;
                    }
                    StartAnimations();
                }
            }
        }

        protected void OnEnable()
        {
            UpdateTimer();
        }

        private void SetupButton()
        {
            if (Button != null)
            {
                Button.onClick.MapListenerWithSound(OnButtonClicked).DisposeWith(this);
            }
        }

        private void OnButtonClicked()
        {
            if (_dailyRewardService.TryGetTodayRewardPreview(out var rewardInfo))
            {
                var context = new DailyRewardPopupContext(
                    rewardInfo.DayIndex,
                    rewardInfo.RewardsByDay,
                    rewardInfo.EarnedDailyReward);
                _flowPopupController.ShowDailyRewardPopup(context);
            }
        }

        private void OnTimerComplete()
        {
            // Timer reached zero - reward is now available
            StartAnimations();
            UpdateTimer(); // Check again to ensure it's still available
        }

        private void StartAnimations()
        {
            if (ButtonTransform == null)
                return;

            // Glow particles
            if (GlowParticles != null)
            {
                GlowParticles.Stop();
                GlowParticles.Play();
            }

            // Shake animation
            ButtonTransform.DOKill();
            ButtonTransform.DOShakePosition(1f, strength: ShakeStrength, vibrato: ShakeVibrato, 
                randomness: ShakeRandomness, snapping: false, fadeOut: false)
                .SetLoops(-1, LoopType.Restart)
                .KillWith(ButtonTransform.gameObject);
        }

        private void StopAnimations()
        {
            if (ButtonTransform != null)
            {
                ButtonTransform.DOKill();
            }

            if (GlowParticles != null)
            {
                GlowParticles.Stop();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            StopAnimations();
        }

        private void OnDestroy()
        {
            StopAnimations();
        }
    }
}
