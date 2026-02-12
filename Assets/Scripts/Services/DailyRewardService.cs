using System;
using System.Collections.Generic;
using Common.Currency;
using Configurations.DailyReward;
using Services;
using Services.Configuration;
using Services.Player;
using Storage.Snapshots;
using Zenject;

public sealed class DailyRewardService
{
    [Inject] private readonly PlayerProfileManager _playerProfileManager;
    [Inject] private readonly PlayerCurrencyService _playerCurrencyService;
    [Inject] private readonly GameConfigurationProvider _configurationProvider;
    [Inject] private readonly BridgeService _bridgeService;

    private DailyRewardConfiguration _config;
    private DailyRewardConfiguration Config => _config ??= _configurationProvider.GetConfig<DailyRewardConfiguration>();

    public bool TryGetTodayRewardPreview(out DailyRewardInfo rewardInfo)
    {
        rewardInfo = default;
        if (!_playerProfileManager.IsInitialized)
            return false;

        var ctx = GetContext();
        if (ctx.isClaimedToday)
            return false;

        int nextDay = CalculateNextDayIndex(ctx.snapshot.CurrentDayIndex, ctx.lastClaimDate, ctx.today);
        var rewards = Config?.GetRewardsForDay(nextDay);

        if (rewards == null || rewards.Count == 0)
            return false;

        rewardInfo = new DailyRewardInfo(nextDay, Config.RewardsByDay, rewards);
        return true;
    }

    public bool TryGetDailyRewardPopupInfo(out DailyRewardInfo info)
    {
        if (TryGetTodayRewardPreview(out info)) return true;
        if (!_playerProfileManager.IsInitialized || Config?.RewardsByDay == null) return false;

        var ctx = GetContext();
        // Если уже забрали, показываем "следующий" день для красоты сетки
        int displayDay = ctx.isClaimedToday
            ? (ctx.snapshot.CurrentDayIndex % DailyRewardConfiguration.CycleLength + 1)
            : CalculateNextDayIndex(ctx.snapshot.CurrentDayIndex, ctx.lastClaimDate, ctx.today);

        info = new DailyRewardInfo(displayDay, Config.RewardsByDay,
            Config.GetRewardsForDay(displayDay) ?? Array.Empty<ICurrency>());
        return true;
    }

    public DailyRewardStatus GetRewardStatus()
    {
        if (!_playerProfileManager.IsInitialized) return new DailyRewardStatus(false, false, TimeSpan.Zero, 0);

        var ctx = GetContext();
        var isMissed = ctx.lastClaimDate < ctx.today.AddDays(-1) && ctx.snapshot.LastClaimUtcTicks > 0;
        var timeUntilNext = ctx.isClaimedToday ? ctx.today.AddDays(1) - _bridgeService.GetServerTime() : TimeSpan.Zero;

        return new DailyRewardStatus(!ctx.isClaimedToday, isMissed, timeUntilNext, ctx.snapshot.CurrentDayIndex);
    }

    public bool TryClaimTodayReward()
    {
        if (!TryGetTodayRewardPreview(out var info))
            return false;

        var ctx = GetContext();
        ctx.snapshot.CurrentDayIndex = info.DayIndex;
        ctx.snapshot.LastClaimUtcTicks = ctx.today.Ticks;
        _playerProfileManager.UpdateDailyRewardAndSave(ctx.snapshot, SavePriority.ImmediateSave);
        return true;
    }

    private int CalculateNextDayIndex(int current, DateTime lastClaim, DateTime today)
    {
        // Упрощенная тернарная логика
        bool isStreakBroken = lastClaim < today.AddDays(-1);
        if (isStreakBroken || lastClaim == DateTime.MinValue) return 1;

        return (current % DailyRewardConfiguration.CycleLength) + 1;
    }

    private (DailyRewardSnapshot snapshot, DateTime today, DateTime lastClaimDate, bool isClaimedToday) GetContext()
    {
        var snapshot = _playerProfileManager.TryGetDailyRewardSnapshot() ?? new DailyRewardSnapshot(0, 0);
        var today = _bridgeService.GetServerTime().Date;
        var lastClaimDate = snapshot.LastClaimUtcTicks > 0
            ? new DateTime(snapshot.LastClaimUtcTicks, DateTimeKind.Utc).Date
            : DateTime.MinValue;

        return (snapshot, today, lastClaimDate, lastClaimDate == today);
    }
}
public readonly struct DailyRewardStatus
{
    public readonly bool IsAvailable;
    public readonly bool IsMissed;
    public readonly TimeSpan TimeUntilNext;
    public readonly int CurrentDayIndex;

    public DailyRewardStatus(bool isAvailable, bool isMissed, TimeSpan timeUntilNext, int currentDayIndex)
    {
        IsAvailable = isAvailable;
        IsMissed = isMissed;
        TimeUntilNext = timeUntilNext;
        CurrentDayIndex = currentDayIndex;
    }
}

public readonly struct DailyRewardInfo
{
    public readonly int DayIndex;
    public readonly IReadOnlyDictionary<int, IList<ICurrency>> RewardsByDay;
    public readonly IList<ICurrency> EarnedDailyReward;

    public DailyRewardInfo(
        int dayIndex,
        IReadOnlyDictionary<int, IList<ICurrency>> rewardsByDay,
        IList<ICurrency> earnedDailyReward)
    {
        DayIndex = dayIndex;
        RewardsByDay = rewardsByDay;
        EarnedDailyReward = earnedDailyReward;
    }
}
