#if UNITY_EDITOR
using System;
using System.Reflection;
using Configurations.DailyReward;
using Editor.Tests;
using NUnit.Framework;
using Services;
using Services.Configuration;
using Services.DailyReward;
using Services.Player;
using Storage.Snapshots;
using UnityEngine;

namespace Tests.Editor
{
    /// <summary>
    /// Builds fake profile snapshots, configs, and test doubles for unit tests.
    /// </summary>
    public class TestBuilderService
    {
        private const string DailyRewardConfigResourceName = "DailyRewardConfiguration";

        private readonly TestProfileSnapshotBuilderService _profileSnapshotBuilder = new TestProfileSnapshotBuilderService();

        public ProfileSnapshot BuildFakeProfileSnapshot(DailyRewardSnapshot initialDailyReward = null)
        {
            return _profileSnapshotBuilder.BuildMinimal(initialDailyReward);
        }

        /// <summary>
        /// Loads DailyRewardConfiguration from Resources (same as production). Returns null if resource is missing.
        /// </summary>
        public DailyRewardConfiguration LoadRealDailyRewardConfig()
        {
            var textAsset = Resources.Load<TextAsset>(DailyRewardConfigResourceName);
            if (textAsset == null)
                return null;
            var config = new DailyRewardConfiguration();
            config.Parse(textAsset.text);
            return config;
        }

        public DailyRewardConfiguration BuildFakeDailyRewardConfig(string csv = null)
        {
            if (string.IsNullOrEmpty(csv))
            {
                csv = "Day; Rewards\n" +
                      "1; Stars 10\n" +
                      "2; Stars 20\n" +
                      "3; Stars 30\n" +
                      "4; Stars 40\n" +
                      "5; Stars 50\n" +
                      "6; Stars 60\n";
            }
            var config = new DailyRewardConfiguration();
            config.Parse(csv);
            return config;
        }

        public TestBridgeService BuildFakeBridge(DateTime? serverTime = null)
        {
            return new TestBridgeService { ServerTime = serverTime ?? DateTime.UtcNow.Date };
        }

        public TestGameConfigurationProvider BuildFakeConfigurationProvider(DailyRewardConfiguration dailyRewardConfig)
        {
            return new TestGameConfigurationProvider(dailyRewardConfig);
        }

        public TestPlayerProfileController BuildFakeProfileController(ProfileSnapshot snapshot)
        {
            var controller = new TestPlayerProfileController(snapshot);
            controller.SetInitialized(true);
            return controller;
        }

        /// <summary>
        /// Builds a fully wired context for daily reward claim flow tests: fake bridge, real config from Resources, providers, and controller.
        /// </summary>
        public DailyRewardFlowContext BuildDailyRewardFlowContext(DateTime startDate)
        {
            var bridge = BuildFakeBridge(startDate);
            var config = LoadRealDailyRewardConfig();
            Assert.IsNotNull(config, $"Resource '{DailyRewardConfigResourceName}' not found in Resources. Add DailyRewardConfiguration to a Resources folder.");
            var configProvider = BuildFakeConfigurationProvider(config);
            var profileSnapshot = BuildFakeProfileSnapshot();
            var profileController = BuildFakeProfileController(profileSnapshot);

            var infoProvider = new DailyRewardsInfoProvider();
            Inject(infoProvider, "_playerProfileController", profileController);
            Inject(infoProvider, "_configurationProvider", configProvider);
            Inject(infoProvider, "_bridgeService", bridge);

            var controller = new DailyRewardController();
            Inject(controller, "_playerProfileController", profileController);
            Inject(controller, "_dailyRewardsInfoProvider", infoProvider);
            Inject(controller, "_bridgeService", bridge);

            return new DailyRewardFlowContext(bridge, config, profileController, infoProvider, controller);
        }

        private static void Inject(object target, string fieldName, object value)
        {
            var type = target.GetType();
            var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            while (field == null && type.BaseType != null)
            {
                type = type.BaseType;
                field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            }
            Assert.IsNotNull(field, $"Field '{fieldName}' not found on {target.GetType().Name}.");
            field.SetValue(target, value);
        }
    }

    public class TestBridgeService : BridgeService
    {
        public DateTime ServerTime { get; set; }

        public void AdvanceDays(int days) => ServerTime = ServerTime.AddDays(days);

        public override DateTime GetServerTime() => ServerTime;
    }

    public class TestGameConfigurationProvider : GameConfigurationProvider
    {
        private readonly DailyRewardConfiguration _dailyRewardConfig;

        public TestGameConfigurationProvider(DailyRewardConfiguration dailyRewardConfig)
        {
            _dailyRewardConfig = dailyRewardConfig;
        }

        public override T GetConfig<T>()
        {
            if (typeof(T) == typeof(DailyRewardConfiguration))
                return _dailyRewardConfig as T;
            return base.GetConfig<T>();
        }
    }

    public class TestPlayerProfileController : PlayerProfileController
    {
        private ProfileSnapshot _testSnapshot;

        public TestPlayerProfileController(ProfileSnapshot initialSnapshot)
        {
            _testSnapshot = initialSnapshot;
        }

        public void SetInitialized(bool value)
        {
            var prop = typeof(PlayerProfileController).GetProperty("IsInitialized",
                BindingFlags.Public | BindingFlags.Instance);
            prop?.SetValue(this, value);
        }

        public override DailyRewardSnapshot TryGetDailyRewardSnapshot() => _testSnapshot?.DailyRewardSnapshot;

        public override void UpdateDailyRewardAndSave(DailyRewardSnapshot dailyRewardSnapshot, SavePriority savePriority)
        {
            if (_testSnapshot == null || dailyRewardSnapshot == null) return;
            _testSnapshot.DailyRewardSnapshot = dailyRewardSnapshot;
        }
    }
}
#endif
