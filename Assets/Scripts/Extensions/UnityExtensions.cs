using UnityEngine;

namespace Extensions
{
    public static class UnityExtensions
    {
        public static TComponent RequireComponent<TComponent>(this GameObject go) where TComponent : Component
        {
            var component = go.GetComponent<TComponent>();
            return component ? component : go.AddComponent<TComponent>();
        }
        
        public static TComponent RequireComponent<TComponent>(this MonoBehaviour go) where TComponent : Component
        {
            if (go == null)
                return null;
            
            var component = go.GetComponent<TComponent>();
            return component ? component : go.gameObject.AddComponent<TComponent>();
        }
        
        public static bool IsDestroyed(this Component component)
        {
            return component == null || component.gameObject == null;
        }
    }
}