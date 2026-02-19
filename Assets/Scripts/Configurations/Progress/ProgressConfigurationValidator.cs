using System;
using System.Collections.Generic;
using System.Linq;

namespace Configurations.Progress
{
    /// <summary>
    /// Validates ProgressConfiguration: time order, unique level ids, stars, figure scale, difficulty.
    /// </summary>
    public static class ProgressConfigurationValidator
    {
        public const int MinDifficulty = 1;
        public const int MaxDifficulty = 3;

        /// <summary>
        /// Validates config. Returns empty list if valid; otherwise list of error messages.
        /// </summary>
        public static IReadOnlyList<string> Validate(ProgressConfiguration config)
        {
            var errors = new List<string>();
            if (config?.PacksInfoDictionary == null)
            {
                errors.Add("Configuration or PacksInfoDictionary is null.");
                return errors;
            }

            var seenLevelKeys = new HashSet<string>();

            foreach (var kvp in config.PacksInfoDictionary)
            {
                int packId = kvp.Key;
                var pack = kvp.Value;
                if (pack == null)
                {
                    errors.Add($"Pack {packId}: PackInfoConfiguration is null.");
                    continue;
                }

                if (pack.StarsToUnlock < 0)
                    errors.Add($"Pack {packId} ({pack.PackName}): StarsToUnlockPack must be >= 0, got {pack.StarsToUnlock}.");

                int packTypeValue = (int)pack.PackType;
                if (packTypeValue != 0 && packTypeValue != 20 && packTypeValue != 30)
                    errors.Add($"Pack {packId} ({pack.PackName}): PackType must be 0 (Default), 20 (Freemium), or 30 (Premium), got {packTypeValue}.");

                if (pack.Levels == null)
                {
                    errors.Add($"Pack {packId}: Levels collection is null.");
                    continue;
                }

                foreach (var level in pack.Levels)
                {
                    if (level == null)
                    {
                        errors.Add($"Pack {packId}: null level entry.");
                        continue;
                    }

                    string levelKey = $"{packId}_{level.LevelId}";
                    if (!seenLevelKeys.Add(levelKey))
                        errors.Add($"Pack {packId}, LevelId {level.LevelId}: duplicate (PackId, LevelId). LevelId must be unique per level.");

                    if (level.BeatingTimes != null && level.BeatingTimes.Length >= 3)
                    {
                        float timeA = level.BeatingTimes[0];
                        float timeB = level.BeatingTimes[1];
                        float timeC = level.BeatingTimes[2];
                        if (!(timeA < timeB && timeB < timeC))
                            errors.Add($"Pack {packId}, LevelId {level.LevelId} ({level.LevelName}): requires TimeA < TimeB < TimeC. Got TimeA={timeA}, TimeB={timeB}, TimeC={timeC}.");
                    }
                    else
                        errors.Add($"Pack {packId}, LevelId {level.LevelId}: BeatingTimes must have at least 3 values (TimeA, TimeB, TimeC).");

                    if (level.FigureScale <= 0f)
                        errors.Add($"Pack {packId}, LevelId {level.LevelId}: FigureScale must be > 0, got {level.FigureScale}.");

                    int diff = (int)level.Difficulty;
                    if (diff < MinDifficulty || diff > MaxDifficulty)
                        errors.Add($"Pack {packId}, LevelId {level.LevelId}: Difficulty must be {MinDifficulty}, {MinDifficulty + 1}, or {MaxDifficulty}, got {diff}.");
                }
            }

            return errors;
        }

        /// <summary>
        /// Throws InvalidOperationException if validation fails.
        /// </summary>
        public static void ValidateOrThrow(ProgressConfiguration config)
        {
            var errors = Validate(config);
            if (errors.Count > 0)
                throw new InvalidOperationException("ProgressConfiguration validation failed: " + string.Join(" ", errors));
        }
    }
}
