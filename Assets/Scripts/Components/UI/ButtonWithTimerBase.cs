using System;
using System.Collections;
using Extensions;
using Installers;
using Services.CoroutineServices;
using TMPro;
using UnityEngine;
using Zenject;

namespace Components.UI
{
    /// <summary>
    /// Simple countdown timer component that displays time remaining.
    /// Start the timer with a duration and it will countdown automatically, updating every second.
    /// </summary>
    public class ButtonWithTimerBase : InjectableMonoBehaviour
    {
        [Inject] protected readonly PersistentCoroutinesService CoroutinesService;

        [Header("Timer UI")]
        [SerializeField] protected TMP_Text TimerText;

        [Header("Timer Settings")]
        [SerializeField] protected string AvailableText = "Available!";

        private Coroutine _timerCoroutine;
        private TimeSpan _remainingTime;
        private Action _onTimerComplete;

        /// <summary>
        /// Starts the timer with the given duration. Timer will countdown automatically, updating every second.
        /// </summary>
        /// <param name="duration">The duration to countdown from</param>
        /// <param name="onComplete">Optional callback when timer reaches zero</param>
        public void StartTimer(TimeSpan duration, Action onComplete = null)
        {
            StopTimer();

            _remainingTime = duration;
            _onTimerComplete = onComplete;

            UpdateTimerDisplay();

            // Only start coroutine if there's time remaining
            if (_remainingTime.TotalSeconds > 0)
            {
                _timerCoroutine = CoroutinesService.StartCoroutine(TimerUpdateCoroutine());
            }
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public void StopTimer()
        {
            if (_timerCoroutine != null)
            {
                CoroutinesService.StopCoroutine(_timerCoroutine);
                _timerCoroutine = null;
            }

            _onTimerComplete = null;
        }

        private IEnumerator TimerUpdateCoroutine()
        {
            while (_remainingTime.TotalSeconds > 0)
            {
                yield return new WaitForSecondsRealtime(1f);

                // Check if component is still enabled and active
                if (!isActiveAndEnabled)
                {
                    yield break;
                }

                _remainingTime = _remainingTime.Subtract(TimeSpan.FromSeconds(1));
                UpdateTimerDisplay();
            }
        }

        private void UpdateTimerDisplay()
        {
            if (TimerText == null)
                return;

            if (_remainingTime.TotalSeconds <= 0)
            {
                TimerText.text = AvailableText;
                _onTimerComplete?.Invoke();
                StopTimer();
            }
            else
            {
                var totalSeconds = (float)_remainingTime.TotalSeconds;
                TimerText.text = DateTimeExtensions.ToClockTime(totalSeconds);
            }
        }

        protected virtual void OnDisable()
        {
            StopTimer();
        }

        protected void OnDestroy()
        {
            StopTimer();
        }
    }
}
