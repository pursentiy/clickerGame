using Common.Currency;
using RSG;
using UnityEngine;

namespace Services.FlyingRewardsAnimation
{
    public class FlyingUIRewardAnimationContext
    {
        public ICurrency[] Rewards { get; } 
        public RectTransform ParentTransform { get; } 
        public Vector3[] RewardPlaces { get; } 
        public Vector3[] TargetsPlaces { get; } 
        public float RewardsMoveTimeSpeedupFactor { get; } 
        public Promise RewardStartFlyPromise { get; }
        public (float ParticlesScale, float ParticleAppearDistanceScale)? SpawnSettings { get; }
        public bool UpdateProfileValues { get; private set; } = true;


        public FlyingUIRewardAnimationContext(
            ICurrency[] rewards,
            RectTransform parentTransform,
            Vector3[] rewardPlaces,
            Vector3[] targetsPlaces, float rewardsMoveTimeSpeedupFactor = 1f,
            (float ParticlesScale, float ParticleAppearDistanceScale)? spawnSettings = null,
            Promise rewardStartFlyPromise = null,
            bool updateProfileValues = true)
        {
            Rewards = rewards;
            ParentTransform = parentTransform;
            RewardPlaces = rewardPlaces;
            TargetsPlaces = targetsPlaces;
            SpawnSettings = spawnSettings;
            RewardsMoveTimeSpeedupFactor = rewardsMoveTimeSpeedupFactor;
            RewardStartFlyPromise = rewardStartFlyPromise;
            UpdateProfileValues = updateProfileValues;

        }
    }
}