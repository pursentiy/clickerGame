using System;
using Handlers;
using Installers;
using UnityEngine.Events;
using UnityEngine.UI;
using Utilities.Disposable;

namespace Extensions
{
    public static class ButtonExtensions
    {
        public static IDisposable MapListenerWithSound(this Toggle.ToggleEvent unityEvent, UnityAction<bool> action, string soundKey = null)
        {
            UnityAction<bool> soundDelegate = (isOn) =>
            {
                var soundHandler = ContainerHolder.CurrentContainer.Resolve<SoundHandler>();
                if (soundKey == null)
                {
                    soundHandler?.PlayButtonSound();
                }
                else
                {
                    soundHandler?.PlaySound(soundKey);
                }
            };
            
            unityEvent.AddListener(soundDelegate);
            unityEvent.AddListener(action);
            
            return new DeferredDisposable(() =>
            {
                unityEvent.RemoveListener(soundDelegate);
                unityEvent.RemoveListener(action);
            });
        }
        
        public static IDisposable MapListenerWithSound(this UnityEvent unityEvent, UnityAction action, string soundKey = null)
        {
            
            var playSound = new UnityAction(() =>
            {
                var soundHandler = ContainerHolder.CurrentContainer.Resolve<SoundHandler>();
                if (soundKey == null)
                {
                    soundHandler?.PlayButtonSound();
                }
                else
                {
                    soundHandler?.PlaySound(soundKey);
                }
            });
            
            unityEvent.AddListener(playSound);
            unityEvent.AddListener(action);
            
            return new DeferredDisposable(() =>
            {
                unityEvent.RemoveListener(playSound);
                unityEvent.RemoveListener(action);
            });
        }
        
        public static IDisposable MapListener(this UnityEvent unityEvent, UnityAction action)
        {
            unityEvent.AddListener(action);
            
            return new DeferredDisposable(() =>
            {
                unityEvent.RemoveListener(action);
            });
        }
    }
}