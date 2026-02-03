using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Services.Base;
using Services.CoroutineServices;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Services.ScreenBlocker
{
    [UsedImplicitly]
    public class UIBlocker<T> : DisposableService where T : UIBlockerComponentBase
    {
        [Inject] private readonly PersistentCoroutinesService _persistentCoroutinesService;
        
        private T _blocker;
        private readonly List<UIBlockRef<T>> _blockRefs = new ();
        
        public bool IsBlocked => _blocker != null && _blocker.isActiveAndEnabled;
        
        public IUIBlockRef Block(float unblockTime = 10)
        {
            var blockRef = new UIBlockRef<T>(this);
            _blockRefs.Add(blockRef);
            
            LogBlockerCreated(blockRef);
            
            if (unblockTime > 0f)
            {
                _persistentCoroutinesService.WaitFor(unblockTime, () => DisposeBlockRefAfterTimer(blockRef));
            }
            
            RefreshState();
            return blockRef;
        }
        
        public void Utilize(UIBlockRef<T> blockRef)
        {
            if (_blockRefs.Remove(blockRef))
            {
                RefreshState();
            }
        }
        
        protected override void OnInitialize()
        {
            _blocker = new GameObject($"[{typeof(T).Name}_Blocker]", typeof(Image), typeof(T)).GetComponent<T>();
            _blocker.Init();
        }

        protected override void OnDisposing()
        {
            Object.Destroy(_blocker.transform.parent.gameObject);
            
            LoggerService.LogDebug(this, "[UIBlocker] Blocker object destroyed");
        }

        private void RefreshState()
        {
            if (_blocker == null)
            {
                LoggerService.LogDebug(this, $"[UIBlocker] [{nameof(RefreshState)}]: blocker is null");
                return;
            }
            
            var blockerObject = _blocker.gameObject;
            blockerObject.SetActive(_blockRefs.Count > 0);
            
            LoggerService.LogDebug(this, $"[UIBlocker] [{nameof(RefreshState)}]: blocked? = {blockerObject.activeSelf} (refs count = {_blockRefs.Count})");
        }

        private static void DisposeBlockRefAfterTimer(UIBlockRef<T> blockRef)
        {
            if (blockRef == null || blockRef.IsDisposed)
            {
                return;
            }
            
            LogBlockerDisposeAfterTimer(blockRef);
            
            blockRef.Dispose();
        }
        
        private static void LogBlockerCreated(UIBlockRef<T> blockRef)
        {
#if UNITY_EDITOR
            LoggerService.LogDebug($"[UIBlocker] blocker Id = {blockRef.Id} at {nameof(LogBlockerCreated)}");
#endif   
        }
        
        private static void LogBlockerDisposeAfterTimer(UIBlockRef<T> blockRef)
        {
#if UNITY_EDITOR
            LoggerService.LogDebug($"[UIBlocker] blocker Id = {blockRef.Id} at {nameof(LogBlockerDisposeAfterTimer)}");
#endif   
        }
    }
}