using Components.UI;
using Handlers;
using Screen.ChoosePack.Widgets;
using Services;
using Storage;
using Storage.Levels.Params;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Screen.ChoosePack
{
    public class ChoosePackScreen : ScreenBase
    {
        [Inject] private ScreenHandler _screenHandler;
        [Inject] private PlayerProgressService _playerProgressService;
        [Inject] private PlayerService _playerService;
        [Inject] private FiguresStorageData _figuresStorageData;
        [Inject] private PlayerCurrencyService _playerCurrencyService;
        
        [SerializeField] private PackItemWidget _packItemWidgetPrefab;
        [SerializeField] private RectTransform _levelEnterPopupsParentTransform;
        [SerializeField] private HorizontalLayoutGroup _horizontalLayoutGroupPrefab;
        [SerializeField] private CurrencyDisplayWidget _starsDisplayWidget;

        private HorizontalLayoutGroup _horizontalGroup;
        private void Start()
        {
            InitializePackButtons();
            _starsDisplayWidget.SetCurrency(_playerCurrencyService.Stars);
        }

        private void InitializePackButtons()
        {
            var currentPackParams = _playerProgressService.GetPackParams();
            var index = 0;
            currentPackParams.ForEach(packParams =>
            {
                if(_horizontalGroup == null || index % 2 == 0)
                    _horizontalGroup = Instantiate(_horizontalLayoutGroupPrefab, _levelEnterPopupsParentTransform);
                
                var enterButton = Instantiate(_packItemWidgetPrefab, _horizontalGroup.transform);
                var isUnlocked = _playerProgressService.IsPackAvailable(packParams.PackNumber);
                var starsRequired = _playerProgressService.GetPackStarsToUnlock(packParams.PackNumber);
                enterButton.Initialize(_figuresStorageData.GetPackParamsData(packParams.PackNumber).PackName, _figuresStorageData.GetPackParamsData(packParams.PackNumber).PackImagePrefab, packParams.PackNumber, isUnlocked,
                    () => TryOpenPack(isUnlocked, packParams), OnUnavailablePackClicked, starsRequired);
                index++;
            });
            
            void OnUnavailablePackClicked()
            {
                _starsDisplayWidget.Bump();
            }

            void TryOpenPack(bool isUnlocked, PackParams packParams)
            {
                if (!isUnlocked)
                    return;
                        
                _playerProgressService.CurrentPackNumber = packParams.PackNumber;
                _screenHandler.ShowChooseLevelScreen();
            }
        }
    }
}