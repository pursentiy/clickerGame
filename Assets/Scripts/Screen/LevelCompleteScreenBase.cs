using System;
using System.Threading.Tasks;
using Handlers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Screen
{
    public class LevelCompleteScreenBase : ScreenBase
    {
        [Inject] private ScreenHandler _screenHandler;
        [Inject] private LevelSessionHandler _levelSessionHandler;
        [Inject] private LevelParamsHandler _levelParamsHandler;
        [Inject] private ProgressHandler _progressHandler;
        
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _tryAgainButton;
        [SerializeField] private RawImage _finalLevelImage;
        [SerializeField] private int _levelTextureWidth;
        [SerializeField] private int _levelTextureHeight;
        
        private Camera _textureCamera;
        private RenderTexture _renderTexture;
        private Action _onFinishLevelSessionAction;

        public void SetOnFinishLevelSessionAction(Action action)
        {
            _onFinishLevelSessionAction = action;
        }

        public void SetupTextureCamera(Camera sourceCamera)
        {
            _textureCamera = sourceCamera;
            
            _textureCamera.gameObject.SetActive(true);
            _renderTexture = new RenderTexture(_levelTextureWidth, _levelTextureHeight, 16, RenderTextureFormat.ARGB32);
            _renderTexture.Create();
            _finalLevelImage.texture = _renderTexture;
            _textureCamera.targetTexture = _renderTexture;
        }
        
        private void Start()
        {
            InitializeLevelsButton();
        }

        private void InitializeLevelsButton()
        {
            _backButton.onClick.AddListener(_screenHandler.ShowChooseLevelScreen);
            _tryAgainButton.onClick.AddListener(TryAgainLevel);
        }

        private void TryAgainLevel()
        {
            if (_currentLevel == -1)
            {
                Debug.LogWarning($"Current Level is {_currentLevel}. Cannot continue. Warning in {this}");
                _screenHandler.ShowChooseLevelScreen();
            }
            
            _progressHandler.ResetLevelProgress(_currentLevel);
            var levelParams = _progressHandler.GetLevelByNumber(_currentLevel);

            TryInvokeFinishLevelSessionAction();
            
            _screenHandler.PopupAllScreenHandlers();
            _levelSessionHandler.StartLevel(levelParams, _levelParamsHandler.LevelHudHandlerPrefab, _levelParamsHandler.TargetFigureDefaultColor);
        }
        
        private void OnDestroy()
        {
            _backButton.onClick.RemoveAllListeners();
            _tryAgainButton.onClick.RemoveAllListeners();
            
            TryInvokeFinishLevelSessionAction();
        }

        private void TryInvokeFinishLevelSessionAction()
        {
            _onFinishLevelSessionAction?.Invoke();
            _onFinishLevelSessionAction = null;
        }
    }
}