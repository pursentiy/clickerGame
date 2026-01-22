using UnityEngine;

namespace Common.Widgets
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeZoneWidget : MonoBehaviour
    {
        private enum BannerPosition
        {
            None = 0,
            Top = 1,
            Bottom = 2
        }

        [SerializeField] private BannerPosition _position = BannerPosition.Top;
        [SerializeField] private float _bannerHeight = 100f;
        [SerializeField] RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            ApplyOffset();
        }

        private void ApplyOffset()
        {
            _rectTransform.anchorMin = Vector2.zero;
            _rectTransform.anchorMax = Vector2.one;

            switch (_position)
            {
                case BannerPosition.Top:
                    _rectTransform.offsetMax = new Vector2(0, -_bannerHeight);
                    _rectTransform.offsetMin = Vector2.zero;
                    break;

                case BannerPosition.Bottom:
                    _rectTransform.offsetMax = Vector2.zero;
                    _rectTransform.offsetMin = new Vector2(0, _bannerHeight);
                    break;

                case BannerPosition.None:
                    _rectTransform.offsetMax = Vector2.zero;
                    _rectTransform.offsetMin = Vector2.zero;
                    break;
            }
        }
    }
}