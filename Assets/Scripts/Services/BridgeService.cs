using System;
using System.Collections;
using Common;
using Playgama;
using Playgama.Modules.Platform;
using RSG;
using UnityEngine;

namespace Services
{
    public class BridgeService
    {
        private const string AuthRememberKey = "was_authorized_once";
        private const float BridgePlatformAwaitTimeout = 5f;

        private bool _isPlatformDetected = false;
        private bool _isGameReadyFlagSet = false;
        
        public bool WasAuthorizedBefore => !CheckPlatform(BridgePlatformType.Mock) && PlayerPrefs.GetInt(AuthRememberKey, 0) == 1;
        public bool ShouldAuthenticatePlayer => CheckPlatform(BridgePlatformType.Yandex) && 
                                                Bridge.player != null && 
                                                !Bridge.player.isAuthorized;
        public bool IsAuthenticated => Bridge.player != null && Bridge.player.isAuthorized;
        
        public bool CheckPlatform(BridgePlatformType platformType)
        {
            return GetCurrentPlatformType() == platformType;
        }

        public void SetGameReady()
        {
            if (_isGameReadyFlagSet)
            {
                LoggerService.LogWarning(this, $"{nameof(PlatformMessage.GameReady)} flag is already set");
                return;
            }

            Bridge.platform.SendMessage(PlatformMessage.GameReady);
            _isGameReadyFlagSet = true;
        }
        
        public IEnumerator PlatformDetectionRoutine()
        {
            if (_isPlatformDetected)
            {
                LoggerService.LogWarning(this, "Bridge platform is already detected");
                yield break;
            }
            
            var timeout = BridgePlatformAwaitTimeout; 
            while (CheckPlatform(BridgePlatformType.Unknown) && timeout > 0)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            LoggerService.LogDebug($"Platform detected: {Bridge.platform.id}");
            _isPlatformDetected = true;
        }

        public IPromise AuthenticatePlayer()
        {
            if (Bridge.player == null)
            {
                LoggerService.LogError(this, "Bridge Player module is not supported on this platform");
                return Promise.Resolved();
            }

            if (Bridge.player.isAuthorized)
            {
                LoggerService.LogDebug(this, "Player is already authorized");
                return Promise.Resolved();
            }
    
            var authenticationPromise = new Promise();
    
            try
            {
                Bridge.player.Authorize(null, success =>
                {
                    if (success)
                    {
                        PlayerPrefs.SetInt(AuthRememberKey, 1);
                        PlayerPrefs.Save();
                        LoggerService.LogDebug(this, "Bridge auth successful");
                    }
                    else
                    {
                        LoggerService.LogWarning(this, "Bridge auth failed or dismissed by user");
                    }
                    
                    authenticationPromise.Resolve();
                });
            }
            catch (Exception e)
            {
                LoggerService.LogError(this, $"Bridge Auth Exception: {e.Message}");
                authenticationPromise.Resolve();
            }

            return authenticationPromise;
        }
        
        private BridgePlatformType GetCurrentPlatformType()
        {
            if (Bridge.platform == null)
            {
                LoggerService.LogWarning(this, "Bridge platform is null");
                return BridgePlatformType.Unknown;
            }
            
            return Bridge.platform.id switch
            {
                GlobalConstants.BridgeYandexId => BridgePlatformType.Yandex,
                GlobalConstants.BridgeGDId => BridgePlatformType.GameDistribution,
                GlobalConstants.BridgeMockId => BridgePlatformType.Mock,
                _ => BridgePlatformType.Unknown
            };
        }
    }

    public enum BridgePlatformType
    {
        Unknown, 
        Mock,
        GameDistribution,
        Yandex
    }
}