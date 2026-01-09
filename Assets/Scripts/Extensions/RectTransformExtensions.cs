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
        
        public static void AdjustScale(this RectTransform self, RectTransform target)
        {
            var selfSize = self.GetCornersSize();
            var targetSize = target.GetCornersSize();
            target.SetLocalScaleXY(Mathf.Min(selfSize.x / targetSize.x, selfSize.y / targetSize.y));
        }
        
        public static void AdjustScaleMax(this RectTransform self, RectTransform target)
        {
            var selfSize = self.GetCornersSize();
            var targetSize = target.GetCornersSize();
            target.SetLocalScaleXY(Mathf.Max(selfSize.x / targetSize.x, selfSize.y / targetSize.y));
        }
        
        public static Vector2 GetCornersSize(this RectTransform rectTransform)
        {
            var corners = new Vector3[4];
            rectTransform.GetLocalCorners(corners);
            return corners[2] - corners[0];
        }
        
        public static void SetUniversalScale(this Transform rt, float scale)
        {
            if (rt != null)
            {
                rt.localScale = new Vector3(scale,scale,scale);
            } 
        }
        
        public static void SetTop(this RectTransform rt, float top)
        {
            rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
        }
    }
}