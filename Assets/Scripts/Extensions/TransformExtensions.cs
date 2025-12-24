using UnityEngine;

namespace Extensions
{
    public static class TransformExtensions
    {
        public static void SetLocalScaleY(this Transform transform, float y = 1)
        {
            var localScale = transform.localScale;
            localScale = new Vector3(localScale.x, y, localScale.z);
            transform.localScale = localScale;
        }
        
        public static void SetLocalScaleXY(this Transform transform, float scale = 1)
        {
            var localScale = transform.localScale;
            localScale = new Vector3(scale, scale, localScale.z);
            transform.localScale = localScale;
        }
    }
}