using UnityEngine;

namespace Common.Widgets
{
    [RequireComponent(typeof(RectTransform))]
    public class FullWidthResizer : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private RectTransform _parentRect;
        
        // Храним последнее известное значение ширины родителя
        private float _lastParentWidth;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _parentRect = transform.parent as RectTransform;
            UpdateSize();
        }

        private void Update()
        {
            // Если родитель внезапно нашелся (если объект был создан динамически)
            if (_parentRect == null)
            {
                _parentRect = transform.parent as RectTransform;
                if (_parentRect == null) return;
            }

            // Сравниваем текущую ширину родителя с сохраненной
            // Мы используем родителя, так как Screen.width в WebGL часто врет 
            // из-за CanvasScaler, а родитель всегда в правильных единицах.
            if (!Mathf.Approximately(_parentRect.rect.width, _lastParentWidth))
            {
                UpdateSize();
            }
        }

        // Это событие Unity UI само дернет UpdateSize при ресайзе окна в браузере
        private void OnRectTransformDimensionsChange()
        {
            UpdateSize();
        }

        public void UpdateSize()
        {
            if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
            if (_parentRect == null) return;

            _lastParentWidth = _parentRect.rect.width;

            // 1. Устанавливаем анкеры в Stretch по горизонтали (0 к 1)
            _rectTransform.anchorMin = new Vector2(0f, _rectTransform.anchorMin.y);
            _rectTransform.anchorMax = new Vector2(1f, _rectTransform.anchorMax.y);

            // 2. Обнуляем отступы (Offsets), чтобы объект точно прилип к краям
            // offsetMin.x = Left, offsetMax.x = -Right
            _rectTransform.offsetMin = new Vector2(0f, _rectTransform.offsetMin.y);
            _rectTransform.offsetMax = new Vector2(0f, _rectTransform.offsetMax.y);

            // 3. Сбрасываем локальную позицию X в 0
            Vector3 pos = _rectTransform.anchoredPosition;
            pos.x = 0;
            _rectTransform.anchoredPosition = pos;

            // 4. На всякий случай сбрасываем масштаб, если какой-то твин его изменил
            _rectTransform.localScale = new Vector3(1f, _rectTransform.localScale.y, 1f);
        }
    }
}