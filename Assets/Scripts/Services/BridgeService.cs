using System.Collections;
using Common;
using Playgama;
using Playgama.Modules.Platform;
using UnityEngine;

namespace Services
{
    public class BridgeService
    {
        private const float BridgePlatformAwaitTimeout = 5f;
        
        private bool _isBridgeAuthComplete = false;
        private bool _isBridgeGameReadyFlagSet = false;

        public void SetGameReady()
        {
            if (_isBridgeGameReadyFlagSet)
            {
                LoggerService.LogWarning(this, $"{nameof(PlatformMessage.GameReady)} flag is already set");
                return;
            }

            Bridge.platform.SendMessage(PlatformMessage.GameReady);
            _isBridgeGameReadyFlagSet = true;
        }
        
        public IEnumerator AuthenticationRoutine()
        {
            if (_isBridgeAuthComplete)
            {
                LoggerService.LogWarning(this, "Bridge auth already complete");
                yield break;
            }
            
            var timeout = BridgePlatformAwaitTimeout; 
            while (Bridge.platform.id == GlobalConstants.BridgeUnknown && timeout > 0)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            LoggerService.LogDebug($"Platform detected: {Bridge.platform.id}");

            if (Bridge.platform.id == GlobalConstants.BridgeGDId)
            {
                _isBridgeAuthComplete = true;
                yield break;
            }

            LoggerService.LogDebug($"[{GetType().Name}] [{nameof(AuthenticationRoutine)}] Platform detected: {Bridge.platform.id}");
            
            while (Bridge.platform.id == GlobalConstants.BridgeUnknown) yield return null;
            
#if UNITY_EDITOR
            LoggerService.LogDebug($"[{GetType().Name}] [{nameof(AuthenticationRoutine)}] Editor detected: Automatic authorization bypass");
            _isBridgeAuthComplete = true;
#else
        Bridge.player.Authorize(null, success =>
            {
                if (success)
                {
                    LoggerService.LogDebug($"[{GetType().Name}] [{nameof(AuthenticationRoutine)}] Successful player authorization.");
                }
                else
                {
                    LoggerService.LogWarning($"[{GetType().Name}] [{nameof(AuthenticationRoutine)}] Guest mode.");
                }
                
                _isBridgeAuthComplete = true;
            });
#endif

            while (!_isBridgeAuthComplete)
            {
                yield return null;
            }
        }
    }
}