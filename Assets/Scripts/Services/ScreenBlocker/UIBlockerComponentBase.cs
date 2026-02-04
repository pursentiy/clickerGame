using Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Services.ScreenBlocker
{
    public abstract class UIBlockerComponentBase : MonoBehaviour
    {
        private Image _image;
        private RectTransform _rectTransform;
        private RectTransform _canvasTransform;
        protected abstract int SortingOrder { get; }

        public void Init()
        {
            var canvasObj = new GameObject($"[{GetType().Name}_Canvas]", typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            var cnvs = canvasObj.GetComponent<Canvas>();
            cnvs.renderMode = RenderMode.ScreenSpaceOverlay;
            cnvs.sortingOrder = SortingOrder;
            
            _canvasTransform = canvasObj.transform as RectTransform;

            DontDestroyOnLoad(canvasObj);
            DontDestroyOnLoad(this.gameObject);
            
            _image = GetComponent<Image>();
            _image.color=new Color(0,0,0,0);
            _rectTransform = transform as RectTransform;
            
            if (_rectTransform != null)
            {
                _rectTransform.SetParent(_canvasTransform);
                _rectTransform.anchorMin = Vector2.zero;
                _rectTransform.anchorMax = Vector2.one;
                _rectTransform.SetTop(0);
                _rectTransform.SetBottom(0);
                _rectTransform.SetRight(0);
                _rectTransform.SetLeft(0);
            }

            gameObject.SetActive(false);

            if (_canvasTransform != null)
            {
                _canvasTransform.SetAsLastSibling();
                _canvasTransform.SetFullStretch();
            }
        }
    }
}