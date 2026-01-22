using Services;
using UnityEngine;

namespace Common.Widgets.ContainerScaler
{
    public class AdaptiveContainerWidget : MonoBehaviour, IScalableWidget
    {
        [SerializeField] private RectTransform _rectTransform;

        [Header("Thresholds")]
        [Tooltip("Aspect ratio below which a screen is considered Ultra Narrow (e.g. 9/21 ≈ 0.43)")]
        [SerializeField] float ultraNarrowThreshold = 0.45f;
        
        [Tooltip("Aspect ratio below which a screen is considered Narrow (e.g. 9/16 = 0.56)")]
        [SerializeField] float narrowThreshold = 0.6f;

        [Header("Ultra Narrow Settings (e.g. 21:9 Phones)")]
        [SerializeField] Vector2 ultraNarrowAnchorMin = new Vector2(0.05f, 0.1f);
        [SerializeField] Vector2 ultraNarrowAnchorMax = new Vector2(0.95f, 0.9f);

        [Header("Narrow Screen Settings (e.g. Standard Phone)")]
        [SerializeField] Vector2 narrowAnchorMin = new Vector2(0.1f, 0.1f);
        [SerializeField] Vector2 narrowAnchorMax = new Vector2(0.9f, 0.9f);

        [Header("Wide Screen Settings (e.g. Tablet/Square)")]
        [SerializeField] Vector2 wideAnchorMin = new Vector2(0.3f, 0.1f);
        [SerializeField] Vector2 wideAnchorMax = new Vector2(0.7f, 0.9f);
        
        public void UpdateWidget(bool byForce = false)
        {
            UpdateAnchors();
        }

        public void AnimateWidget(bool enable) { }

        private void UpdateAnchors()
        {
            if (_rectTransform == null)
            {
                LoggerService.LogWarning(this, $"{nameof(UpdateAnchors)} No rectTransform assigned");
                return;
            }
            
            var currentAspect = (float)UnityEngine.Device.Screen.width / UnityEngine.Device.Screen.height;

            // Проверка идет от самого узкого к широкому
            if (currentAspect <= ultraNarrowThreshold)
            {
                ApplyAnchors(ultraNarrowAnchorMin, ultraNarrowAnchorMax);
            }
            else if (currentAspect <= narrowThreshold)
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

                _rectTransform.offsetMin = Vector2.zero;
                _rectTransform.offsetMax = Vector2.zero;
            }
        }
    }
}