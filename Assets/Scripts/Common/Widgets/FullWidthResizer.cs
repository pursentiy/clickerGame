using UnityEngine;

namespace Common.Widgets
{
    [RequireComponent(typeof(RectTransform))]
    public class FullWidthResizer : MonoBehaviour
    {
        private RectTransform _rectTransform;
        
        private int _lastScreenWidth;
        private int _lastScreenHeight;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            UpdateSize();
        }

        private void Update()
        {
            // Проверяем изменение разрешения экрана
            if (UnityEngine.Device.Screen.width != _lastScreenWidth || UnityEngine.Device.Screen.height != _lastScreenHeight)
            {
                UpdateSize();
            }
        }

        public void UpdateSize()
        {
            if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();

            _lastScreenWidth = UnityEngine.Device.Screen.width;
            _lastScreenHeight = UnityEngine.Device.Screen.height;

            // Устанавливаем анкеры по центру горизонтали, чтобы избежать конфликтов
            _rectTransform.anchorMin = new Vector2(0.5f, _rectTransform.anchorMin.y);
            _rectTransform.anchorMax = new Vector2(0.5f, _rectTransform.anchorMax.y);

            // Выставляем ширину равную текущему разрешению экрана
            // Если вы используете CanvasScaler, Screen.width автоматически 
            // адаптируется под reference resolution канваса.
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _lastScreenWidth);
            
            // Если нужно, чтобы объект был всегда в нуле по X
            Vector2 pos = _rectTransform.anchoredPosition;
            pos.x = 0;
            _rectTransform.anchoredPosition = pos;
        }

        // Вызывается автоматически при изменении параметров в инспекторе
        private void OnValidate()
        {
            UpdateSize();
        }
    }
}