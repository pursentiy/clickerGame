using System;
using System.Collections.Generic;
using System.Linq;
using Common.Currency;
using Common.Rewards;
using Extensions;
using RSG;
using Services.ContentDeliveryService;
using UnityEngine;
using Utilities.Disposable;
using Zenject;

namespace Services.FlyingRewardsAnimation
{
    public class FlyingUIRewardAnimationService
    {
        [Inject] private FlyingUIRewardDestinationService _destinationService;
        
        public IPromise Play(FlyingUIRewardAnimationContext context)
        {
            if (context == null)
            {
                LoggerService.LogError($"{GetType().Name}: {nameof(FlyingUIRewardAnimationContext)} at {nameof(Play)} is null");
                return Promise.Resolved();
            }

            return FlyToDestination(context);
        }
        
        public IPromise PlayAnimation(FlyingUIRewardAnimationContext context)
        {
            try
            {
                if (context == null)
                {
                    LoggerService.LogError(
                        $"{GetType().Name}: {nameof(FlyingUIRewardAnimationContext)} at {nameof(PlayAnimation)} is null");
                    return Promise.Resolved();
                }

                var promises = new List<IPromise>();
                
                var disposableRewardsContent = new List<IDisposableContent<GameObject>>();

                if (context.ParentTransform == null)
                {
                    context.RewardStartFlyPromise?.SafeResolve();
                    return Promise.Resolved();
                }

                var isAnyRewards = false;

                for (var i = 0; i < context.Rewards.Length; i++)
                {

                    isAnyRewards = true;

                    var i1 = i;
                    promises.Add(_currencyLibraryService.InstantiateAsync2DRewardAnimation(context.ParentTransform)
                        .DisposeResultWith(context.ParentTransform)
                        .CancelWith(context.ParentTransform.gameObject.GetDisposeProvider())
                        .Then(disposableContent =>
                        {
                            context.RewardStartFlyPromise?.SafeResolve();

                            if (disposableContent.Asset == null)
                                return Promise.Resolved();

                            disposableRewardsContent.Add(disposableContent);
                            var rewardAnimationObject = disposableContent.Asset;
                            rewardAnimationObject.RequireComponent<CanvasGroup>().ignoreParentGroups = true;

                            var startingPosition = context.RewardPlaces.Length > i1 ? context.RewardPlaces[i1] : context.RewardPlaces.First();
                            rewardAnimationObject.transform.position = startingPosition;

                            var animation = rewardAnimationObject.GetComponent<RewardAnimation>();
                            if (animation == null)
                                return Promise.Resolved();

                            var particlesScale = 1f; 
                            var particleAppearDistanceScale = 1f;
                            var currency = context.Rewards[i1];
                            
                            if (context.SpawnSettings.HasValue)
                            {
                                particlesScale = context.SpawnSettings.Value.ParticlesScale;
                                particleAppearDistanceScale = context.SpawnSettings.Value.ParticleAppearDistanceScale;
                            }
                            
                            animation.SetRewardsSprite(GetRewardSprite(currency));
                            
                            return (context.TargetsPlaces == null || context.TargetsPlaces.Length == 0 
                                ? _destinationService.GetDestination(currency) 
                                : Promise<Vector3>.Resolved(context.TargetsPlaces.Length > i1 ? context.TargetsPlaces[i1] : context.TargetsPlaces.First()))
                            .Then(targetPos =>
                            {
                                switch (currency)
                                {

                                    case Stars stars:
                                        var starsRewardAnimationPromises = animation
                                            .AnimateAndWaitFor(
                                                startingPosition,
                                                targetPos,
                                                particlesScale, 
                                                stars,
                                                context.RewardsMoveTimeSpeedupFactor,
                                                particleSpacing: particleAppearDistanceScale,
                                                callbackForEachAnimation: () =>
                                                {
                                                    _uiManager.GetPlayerHUDWidget()?.UpdateStarsValueWithAnimation(stars.Value, 0.3f);
                                                });

                                        starsRewardAnimationPromises.AllParticlesFinish
                                            .Then(() =>
                                            {
                                                _playerService.UpdateStarsSilentlySignal.Dispatch(true);
                                                _currencyService.Add<Stars>(animation.PoolCount > stars ? 0 : stars - animation.PoolCount);
                                                _playerService.UpdateStarsSilentlySignal.Dispatch(false);
                                            });

                                        return starsRewardAnimationPromises.AllParticlesFinish;
                                    

                                    default:
                                        return animation
                                            .AnimateAndWaitAll(
                                                startingPosition,
                                                targetPos,
                                                particlesScale.AsMaybe(),
                                                currency.GetCount(),
                                                context.RewardsMoveTimeSpeedupFactor,
                                                particleSpacing: particleAppearDistanceScale)
                                            .ContinueWithResolved(() =>
                                            {
                                                if (context.UpdateProfileValues)
                                                {
                                                    _currencyService.Visualize(currency);
                                                    _currencyService.Add(currency);
                                                }
                                            });
                                }
                            })
                            .CancelWith(context.ParentTransform.gameObject.GetDisposeProvider());
                        }));
                }

                _soundService.Play(context.Rewards, context.RewardsMoveTimeSpeedupFactor);

                if (!isAnyRewards)
                {
                    context.RewardStartFlyPromise?.SafeResolve();
                    context?.ChestFly?.StartFly?.SafeResolve();
                }
                
                if (context.UpdateProfileValues && spinsPromises.Any())
                {
                    Promise<Spins>.All(spinsPromises)
                        .Then(spins =>
                        {
                            var spinsFlat = spins.Aggregate((a, b) => a + b);
                            _currencyService.Visualize<Spins>(spinsFlat);
                            _currencyService.Add<Spins>(spinsFlat);
                        });
                }

                var animationPromise = Promise.All(promises)
                    .CancelWith(context.ParentTransform.gameObject.GetDisposeProvider());

                animationPromise.Finally(() => DisposeRewardsContent(disposableRewardsContent));

                return animationPromise;
            }
            catch (Exception e)
            {
                _silentErrorHandler.HandleDevSilentError(e.ToString());

                return Promise.Resolved();
            }
        }
        
        private Sprite GetRewardSprite(ICurrency currency)
        {
            return currency is Coins 
                ? _currencyLibraryService.GetSingleCoinSprite()
                : _currencyLibraryService.GetMainIcon(currency);
        }
        
        private IPromise FlyToDestination(FlyingUIRewardAnimationContext context)
        {
            if (context == null || context.Rewards.IsNullOrEmpty())
            {
                return Promise.Resolved();
            }
            
            try
            {
                if (context.ParentTransform == null || context.ParentTransform.gameObject == null)
                {
                    return Promise.Resolved();
                }
                
                var result = new Promise(this);
                var particleScale = context.MaybeParticleScale.HasValue ? context.MaybeParticleScale.Value : 1f;
                var particleDistanceScale = context.ParticleSpacing;

                var animContext = new FlyingUIRewardAnimationContext(
                    context.Rewards.ToArray(),
                    context.ParentTransform,
                    context.RewardPlaces.ToArray(),
                    targetsPlaces: context.CustomDestination.HasValue ? new [] { context.CustomDestination.Value } : null,
                    context.RewardsMoveTimeSpeedupFactor,
                    (particleScale, particleDistanceScale),
                    updateProfileValues: context.UpdateProfileValues);
                
                var spawnPromise = PlayAnimation(animContext)
                    .CancelWith(context.ParentTransform.gameObject.GetDisposeProvider());
                
                spawnPromise.Finally(() =>
                {
                    result.SafeResolve();
                });

                return result;
            }
            catch (Exception e)
            {
                LoggerService.LogError( e.ToString());
                return Promise.Resolved();
            }
        }
    }
}