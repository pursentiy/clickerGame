using System;
using Services;
using UnityEngine;

namespace Extensions
{
    public static class ActionExtensions
    {
        public static void SafeInvoke(this Action e)
        {
            try
            {
                e?.Invoke();
            }
            catch (Exception exception)
            {
                LoggerService.LogError($"Error invoking Action {TryGetDebugInfo(e)}: {exception}");
            }
        }
        
        public static string TryGetDebugInfo(this Delegate e)
        {
            var result = string.Empty;
            
            try
            {
                result += e.Method.Name;

                if (e.Method.DeclaringType != null)
                {
                    result.AddIfNonEmpty("/");
                    result += $"{e.Method.DeclaringType.Name}";
                }
                
                if (e.Target != null)
                {
                    result.AddIfNonEmpty("/");
                    result += $"{e.Target.GetType().Name}";
                }
            }
            catch (Exception)
            {
                // ignore
            }

            return result;
        }
        
        public static void SafeInvoke<T>(this Action<T> e, T arg)
        {
            try
            {
                e?.Invoke(arg);
            }
            catch (Exception exception)
            {
                LoggerService.LogError($"Error invoking Action {TryGetDebugInfo(e)}: {exception}");
            }
        }
    }
}