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
        
        public static void SetFullStretch(this RectTransform rt)
        {
            rt.anchorMin=Vector2.zero;
            rt.anchorMax=Vector2.one;
            rt.SetUniversalScale(1);
            rt.SetLeft(0);
            rt.SetTop(0);
            rt.SetRight(0);
            rt.SetBottom(0);
        }
        
        public static void SetTop(this RectTransform rt, float top)
        {
            rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
        }
        
        public static void SetLeft(this RectTransform rt, float left)
        {
            rt.offsetMin = new Vector2(left, rt.offsetMin.y);
        }
        
        public static void SetRight(this RectTransform rt, float right)
        {
            rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
        }
        
        public static void SetBottom(this RectTransform rt, float bottom)
        {
            rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
        }
    }
}