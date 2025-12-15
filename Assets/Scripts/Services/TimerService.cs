using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Services
{
    public class TimerService : MonoBehaviour
    {
        private Dictionary<string, Timer> _activeTimers = new ();
        
        public Timer StartTimer(string id, float duration, Action onComplete = null, Action<float> onUpdate = null)
        {
            if (_activeTimers.ContainsKey(id))
            {
                Debug.LogWarning($"Timer with ID {id} already exists. Stopping old one.");
                _activeTimers[id].Dispose();
            }

            var newTimer = new Timer(id, duration, onComplete, onUpdate);
            _activeTimers.Add(id, newTimer);
            
            newTimer.Coroutine = StartCoroutine(RunTimerRoutine(newTimer));
        
            return newTimer;
        }

        public Timer GetTimer(string id)
        {
            return _activeTimers.GetValueOrDefault(id);
        }
        
        public void DeregisterTimer(string id)
        {
            if (_activeTimers.ContainsKey(id))
            {
                _activeTimers.Remove(id);
            }
        }
        
        private IEnumerator RunTimerRoutine(Timer timer)
        {
            float elapsed = 0f;

            while (elapsed < timer.Duration)
            {
                if (timer.IsPaused)
                {
                    yield return null;
                    continue;
                }

                elapsed += Time.deltaTime;
                timer.TimeRemaining = Mathf.Max(0, timer.Duration - elapsed);
                timer.OnUpdate?.Invoke(timer.TimeRemaining);

                yield return null;
            }
            
            StopCoroutine(timer.Coroutine);
            DeregisterTimer(timer.Id);
            timer.Complete();
        }
    }
    
    public class Timer : IDisposable
    {
        public string Id { get; private set; }
        public float Duration { get; private set; }
        public float TimeRemaining { get; set; }
        public bool IsPaused { get; private set; }
        
        public Action<float> OnUpdate;
        public Coroutine Coroutine;
        
        private bool _isDisposed;
        private Action _onComplete;

        public Timer(string id, float duration, Action onComplete, Action<float> onUpdate)
        {
            Id = id;
            Duration = duration;
            TimeRemaining = duration;
            OnUpdate = onUpdate;
            
            _onComplete = onComplete;
        }

        public void Pause() => IsPaused = true;
        public void Resume() => IsPaused = false;
        
        public void Complete()
        {
            if (_isDisposed) 
                return;
        
            _onComplete?.Invoke();
            Dispose();
        }
        
        public void Dispose()
        {
            if (_isDisposed) return;

            _onComplete = null;
            OnUpdate = null;
            _isDisposed = true;
        }
    }
}