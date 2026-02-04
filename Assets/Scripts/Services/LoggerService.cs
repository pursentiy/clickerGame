using System;
using UnityEngine;
using Object = System.Object;

namespace Services
{
    public static class LoggerService
    {
        public static void LogDebug(Object @object, string message)
        {
            LogDebug(@object.GetType().Name + ": " + message);
        }
        
        public static void LogDebug(string message)
        {
            Debug.Log(message);
        }
        
        public static void LogDebugEditor(string message)
        {
#if UNITY_EDITOR
            Debug.Log(message);
#endif
        }

        public static void LogWarningEditor(string message)
        {
#if UNITY_EDITOR
            LogWarning(message);
#endif
        }
        
        public static void LogDebugEditor(Object @object, string message)
        {
#if UNITY_EDITOR
            LogDebugEditor(@object.GetType().Name +  ": " + message);
#endif
        }
        
        public static void LogWarning(Object @object, string message)
        {
            LogWarning(@object.GetType().Name +  ": " + message);
        }
        
        public static void LogWarning(Exception exception)
        {
            LogWarning(exception.Message);
        }
        
        public static void LogError(Object @object, string message)
        {
            Debug.LogError(@object.GetType().Name +  ": " + message);
        }
        
        public static void LogError(string message)
        {
            Debug.LogError(message);
        }
        
        public static void LogError(Exception exception)
        {
            Debug.LogError(exception);
        }
        
        public static void LogWarning(string message)
        {
            Debug.LogWarning(message);
        }
    }
}