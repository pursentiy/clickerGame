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
        
        public static string ToStopwatchTime(double seconds)
        {
            // Предотвращаем отрицательные значения
            seconds = Math.Max(0, seconds);

            var t = TimeSpan.FromSeconds(seconds);
        
            // Получаем сотые доли (00-99)
            // Делим на 10, так как Milliseconds возвращает 0-999
            int hundredths = t.Milliseconds / 10;

            // Case 1: Есть дни (Format: D:H:M:S:ms)
            if (t.TotalDays >= 1)
            {
                return $"{t.Days}:{t.Hours}:{t.Minutes:D2}:{t.Seconds:D2}:{hundredths:D2}";
            }
    
            // Case 2: Есть часы (Format: H:M:S:ms)
            if (t.TotalHours >= 1)
            {
                return $"{t.Hours}:{t.Minutes:D2}:{t.Seconds:D2}:{hundredths:D2}";
            }

            // Case 3: Минуты, секунды и сотые (Format: M:S:ms)
            // Используем (int)t.TotalMinutes, чтобы корректно отображать минуты > 60, если нужно
            return $"{(int)t.TotalMinutes}:{t.Seconds:D2}:{hundredths:D2}";
        }
    }
}