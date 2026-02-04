using System.Collections;
using Extensions;
using Handlers.UISystem.Popups;
using Handlers.UISystem.Screens;
using Services;
using Services.Base;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Handlers.UISystem
{
    public sealed class UIManager : DisposableMonoBehaviourService
    {
        [Inject] private readonly UISystemData _settings;
        [Inject] private readonly UIPopupsHandler _popupsHandler;
        [Inject] private readonly UIScreensHandler _screensHandler;
        //TODO ADD UIScreensHander

        public Canvas RootCanvas => _uiCanvas;
        public UIPopupsHandler PopupsHandler => _popupsHandler;
        public UIScreensHandler ScreensHandler => _screensHandler;
        public Canvas ScreensCanvas => _screensCanvas;
        public Canvas PreloaderCanvas => _preloaderCanvas;

        public float UIScale { get; private set; } = 1;

        public const float MinAspectRatio = 0f;
        public const float MaxAspectRatio = 3f;

        private Canvas _uiCanvas;
        private GraphicRaycaster _uiCanvasRaycaster;
        private CanvasScaler _uiCanvasScaler;
        private Canvas _popupsCanvas;
        private Canvas _foregroundPopupsCanvas;
        private Canvas _notificationsCanvas;
        private Canvas _screensCanvas;
        private Canvas _preloaderCanvas;
        private Canvas _navigationCanvas;

        private CanvasGroup _screensCanvasGroup;
        private CanvasGroup _navigationCanvasGroup;

        protected override void OnInitialize()
        {
            SetupCanvases();
        }

        public void ShowScreensUI()
        {
            SetScreensUIVisibility(true);
        }

        public void HideScreensUI()
        {
            SetScreensUIVisibility(false);
        }

        public void SetScreensUIInteractivity(bool isInteractable)
        {
            _screensCanvasGroup.interactable = isInteractable;
            
            LoggerService.LogDebug($"UI interactivity set to {isInteractable}");
        }

        public void SetupHandlers()
        {
            _popupsHandler.Initialize(_popupsCanvas);
            _screensHandler.Initialize(_screensCanvas, CoroutineProviderCallback);
        }

        private void SetScreensUIVisibility(bool isVisible)
        {
            _screensCanvasGroup.alpha = isVisible ? 1f : 0f;
            SetScreensUIInteractivity(isVisible);
        }

        private void CoroutineProviderCallback(IEnumerator routine)
        {
            StartCoroutine(routine);
        }

        private void SetupCanvases()
        {
            var uiCanvas = Instantiate(_settings.UICanvasPrefab);
            _uiCanvas = uiCanvas.GetComponent<Canvas>();
            _uiCanvasScaler = uiCanvas.GetComponent<CanvasScaler>();
            _uiCanvasRaycaster = uiCanvas.GetComponent<GraphicRaycaster>();
                
            _uiCanvas.gameObject.name = "UICanvas";
            
            InitUIScale();

            var uiCanvasTransform = _uiCanvas.GetRectTransform();
            
            _screensCanvas = SpawnCanvas(_settings.UICanvasPrefab, uiCanvasTransform, "ScreensCanvas");
            
            _preloaderCanvas = SpawnCanvas(_settings.UICanvasPrefab, uiCanvasTransform, "PreloaderCanvas");
            _preloaderCanvas.overrideSorting = true;
            _preloaderCanvas.sortingOrder = 110;
            
            _navigationCanvas = SpawnCanvas(_settings.UICanvasPrefab, uiCanvasTransform, "NavigationCanvas");
            _navigationCanvas.overrideSorting = true;
            _navigationCanvas.sortingOrder = 111;
            _navigationCanvas.enabled = true;
            _navigationCanvasGroup = _navigationCanvas.GetComponent<CanvasGroup>();
            _navigationCanvasGroup.alpha = 0f;
            
            _popupsCanvas = SpawnCanvas(_settings.UICanvasPrefab, uiCanvasTransform, "PopupsCanvas");
            _popupsCanvas.overrideSorting = true;
            _popupsCanvas.sortingOrder = 113;
            
            _foregroundPopupsCanvas = SpawnCanvas(_settings.UICanvasPrefab, uiCanvasTransform, "ForegroundPopupsCanvas");
            _foregroundPopupsCanvas.overrideSorting = true;
            _foregroundPopupsCanvas.sortingOrder = 114;
            
            _notificationsCanvas = SpawnCanvas(_settings.UICanvasPrefab, uiCanvasTransform, "NotificationsCanvas");
            _notificationsCanvas.overrideSorting = true;
            _notificationsCanvas.sortingOrder = 115;
            
            _screensCanvasGroup = _screensCanvas.GetComponent<CanvasGroup>();
        }

        private void InitUIScale()
        {
            var uiCanvasTransform = _uiCanvas.GetRectTransform();
            var size = uiCanvasTransform.sizeDelta;
            var aspectRatio = size.x / size.y;
            if (aspectRatio < MinAspectRatio)
            {
                UIScale = size.x / (size.y * MinAspectRatio);
            }
        }
        
        private Canvas SpawnCanvas(GameObject prefab, RectTransform parent, string canvasName)
        {
            var canvas = Instantiate(prefab, parent).GetComponent<Canvas>();
            canvas.gameObject.name = canvasName;
    
            canvas.GetComponent<RectTransform>().SetFullStretch();
            
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null) Destroy(scaler);

            return canvas;
        }

        protected override void OnDisposing()
        {
            StopAllCoroutines();
            _popupsHandler.Dispose();
            _screensHandler.Dispose();
            
            _uiCanvasScaler.enabled = false;
            _uiCanvasRaycaster.enabled = false;
            
            Destroy(_uiCanvas.gameObject);
        }
    }
}