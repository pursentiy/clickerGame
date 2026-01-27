using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Services.Base;
using Services.CoroutineServices;
using UnityEngine;
using Zenject;

namespace Services
{
    public class TimeService : DisposableService
    {
        [Inject] private PersistentCoroutinesService _persistentCoroutinesService;
        
        private Dictionary<string, Timer> _activeTimers = new Dictionary<string, Timer>();
        
        // --- 1. Standard Countdown Timer ---
        public Timer StartTimer(string id, float duration, Action onComplete = null, Action<double> onUpdate = null)
        {
            return CreateTimerInternal(id, duration, false, onComplete, onUpdate);
        }

        // --- 2. Stopwatch (Counts Up) ---
        public Timer StartStopwatch(string id, Action<double> onUpdate = null)
        {
            // Stopwatches have no fixed duration, so we pass 0 or ignored value
            return CreateTimerInternal(id, 0f, true, null, onUpdate);
        }

        private Timer CreateTimerInternal(string id, float duration, bool isStopwatch, Action onComplete,
            Action<double> onUpdate)
        {
            if (_activeTimers.ContainsKey(id))
            {
                LoggerService.LogWarning($"Timer '{id}' exists. Overwriting.");
                StopTimer(id);
            }

            if (HasMultipleActiveStopwatches())
            {
                LoggerService.LogWarning(this, $"[{nameof(CreateTimerInternal)}] Multiple active stopwatches detected.");
            }

            Timer newTimer = new Timer(id, duration, isStopwatch, onComplete, onUpdate);
            _activeTimers.Add(id, newTimer);

            newTimer.Coroutine = _persistentCoroutinesService.StartCoroutine(RunTimerRoutine(newTimer));
            return newTimer;
        }

        private bool HasMultipleActiveStopwatches()
        {
            if (_activeTimers.IsCollectionNullOrEmpty())
                return false;

            return _activeTimers.Count(i => i.Value is { IsStopwatch: true, IsDisposed: false }) > 1;
        }

        public Timer GetTimer(string id)
        {
            return _activeTimers.GetValueOrDefault(id);
        }

        public void StopTimer(string id)
        {
            if (!_activeTimers.TryGetValue(id, out var timer)) 
                return;

            if (timer == null)
            {
                LoggerService.LogWarning($"Timer '{id}' does not exist.");
                _activeTimers.Remove(id);
                return;
            }
            
            if (timer.Coroutine != null)
                _persistentCoroutinesService.StopCoroutine(timer.Coroutine);
                
            timer.Dispose();
            _activeTimers.Remove(id);
        }
        
        protected override void OnInitialize()
        {
            
        }

        protected override void OnDisposing()
        {
            StopAllTimers();
        }
        
        private void StopAllTimers()
        {
            if (_activeTimers.IsNullOrEmpty())
            {
                return;
            }
            
            _activeTimers.Select(i => i.Key).ToList().ForEach(StopTimer);
        }

        private IEnumerator RunTimerRoutine(Timer timer)
        {
            // Stopwatch loop (infinite until disposed)
            if (timer.IsStopwatch)
            {
                while (!timer.IsDisposed)
                {
                    if (!timer.IsPaused)
                    {
                        timer.Elapsed += Time.deltaTime;
                        timer.OnUpdate?.Invoke(timer.Elapsed); // Pass Elapsed Time
                    }

                    yield return null;
                }
            }
            // Countdown loop (runs until duration reached)
            else
            {
                while (timer.Elapsed < timer.Duration && !timer.IsDisposed)
                {
                    if (!timer.IsPaused)
                    {
                        timer.Elapsed += Time.deltaTime;
                        float remaining = Mathf.Max(0, timer.Duration - timer.Elapsed);
                        timer.OnUpdate?.Invoke(remaining); // Pass Remaining Time
                    }

                    yield return null;
                }

                if (!timer.IsDisposed)
                {
                    _persistentCoroutinesService.StopCoroutine(timer.Coroutine);
                    StopTimer(timer.Id);
                    timer.Complete();
                }
            }
        }
    }

    public class Timer : IDisposable
    {
        public string Id { get; private set; }
        public float Duration { get; private set; }
        public bool IsStopwatch { get; private set; }
        public float Elapsed { get; set; } // Tracks time passed for both modes
        public bool IsPaused { get; set; }
        public bool IsDisposed { get; private set; }

        public Action OnComplete;
        public Action<double> OnUpdate; // Timer = Remaining, Stopwatch = Elapsed

        public Coroutine Coroutine;

        public Timer(string id, float duration, bool isStopwatch, Action onComplete, Action<double> onUpdate)
        {
            Id = id;
            Duration = duration;
            IsStopwatch = isStopwatch;
            OnComplete = onComplete;
            OnUpdate = onUpdate;
            Elapsed = 0f;
        }

        public void Pause() => IsPaused = true;
        public void Resume() => IsPaused = false;
        public void Reset() => Elapsed = 0f;

        public void Complete()
        {
            if (IsDisposed) return;
            OnComplete?.Invoke();
            Dispose();
        }

        public void Dispose()
        {
            if (IsDisposed) 
                return;

            Reset();
            Duration = 0;
            IsDisposed = true;
            OnComplete = null;
            OnUpdate = null;
        }
    }
}