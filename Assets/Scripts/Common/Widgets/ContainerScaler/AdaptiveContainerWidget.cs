using Services;
using UnityEngine;

namespace Common.Widgets.ContainerScaler
{
    public class AdaptiveContainerWidget : MonoBehaviour, IScalableWidget
    {
        [SerializeField] private RectTransform _rectTransform;

        [Header("Threshold")]
        [Tooltip("The aspect ratio below which a screen is considered narrow (e.g. 9/16 = 0.56)")]
        public float aspectThreshold = 0.6f;

        [Header("Narrow Screen Settings (e.g. Phone)")]
        public Vector2 narrowAnchorMin = new Vector2(0.1f, 0.1f);

        public Vector2 narrowAnchorMax = new Vector2(0.9f, 0.9f);

        [Header("Wide Screen Settings (e.g. Tablet/Square)")]
        public Vector2 wideAnchorMin = new Vector2(0.3f, 0.1f);

        public Vector2 wideAnchorMax = new Vector2(0.7f, 0.9f);
        
        public void UpdateWidget(bool byForce = false)
        {
            UpdateAnchors();
        }

        public void AnimateWidget(bool enable)
        {
            
        }

        private void UpdateAnchors()
        {
            if (_rectTransform == null)
            {
                LoggerService.LogWarning(this,  $"{nameof(UpdateAnchors)} No rectTransform assigned");
                return;
            }
            
            var currentAspect = (float)UnityEngine.Device.Screen.width / UnityEngine.Device.Screen.height;

            if (currentAspect <= aspectThreshold)
            {
                ApplyAnchors(narrowAnchorMin, narrowAnchorMax);
            }
            else
            {
                ApplyAnchors(wideAnchorMin, wideAnchorMax);
            }
        }

        private void ApplyAnchors(Vector2 min, Vector2 max)
        {
            if (_rectTransform.anchorMin != min || _rectTransform.anchorMax != max)
            {
                _rectTransform.anchorMin = min;
                _rectTransform.anchorMax = max;

                // Сбрасываем offset, чтобы контейнер точно прилип к анкорам
                _rectTransform.offsetMin = Vector2.zero;
                _rectTransform.offsetMax = Vector2.zero;
            }
        }
    }
}