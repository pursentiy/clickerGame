using System;
using UnityEngine;

namespace Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToClockTime(float seconds)
        {
            // Prevent negative time glitches
            seconds = Mathf.Max(0, seconds);

            var t = TimeSpan.FromSeconds(seconds);

            // Case 1: Has Days (Format: D:HH:MM:SS)
            if (t.TotalDays >= 1)
            {
                return $"{t.Days}:{t.Hours:D2}:{t.Minutes:D2}:{t.Seconds:D2}";
            }
        
            // Case 2: Has Hours (Format: H:MM:SS)
            if (t.TotalHours >= 1)
            {
                return $"{t.Hours}:{t.Minutes:D2}:{t.Seconds:D2}";
            }

            // Case 3: Minutes and Seconds only (Format: M:SS)
            // We use (int)t.TotalMinutes to support cases like "59:59" vs "1:00"
            return $"{(int)t.TotalMinutes}:{t.Seconds:D2}";
        }
    }
}