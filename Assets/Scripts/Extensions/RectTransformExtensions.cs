using UnityEngine;

namespace Extensions
{
    public static class RectTransformExtensions
    {
        public static RectTransform GetRectTransform(this Component component)
        {
            return component.transform as RectTransform;
        }

        public static RectTransform GetRectTransform(this MonoBehaviour component)
        {
            return component.transform as RectTransform;
        }
        
        public static RectTransform GetRectTransform(this GameObject component)
        {
            return component.transform as RectTransform;
        }
    }
}