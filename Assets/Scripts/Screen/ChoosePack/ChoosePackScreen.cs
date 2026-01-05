using System.Collections.Generic;
using Components.UI;
using Handlers;
using Screen.ChoosePack.Widgets;
using Services;
using Storage;
using Storage.Levels;
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
        [Inject] private LevelsParamsStorageData _levelsParamsStorageData;
        [Inject] private PlayerCurrencyService _playerCurrencyService;
        
        [SerializeField] private PackItemWidget _packItemWidgetPrefab;
        [SerializeField] private RectTransform _levelEnterPopupsParentTransform;
        [SerializeField] private HorizontalLayoutGroup _horizontalLayoutGroupPrefab;
        [SerializeField] private CurrencyDisplayWidget _starsDisplayWidget;

        private List<HorizontalLayoutGroup> _horizontalGroups = new();
        private void Start()
        {
            InitializePackButtons();
            _starsDisplayWidget.SetCurrency(_playerCurrencyService.Stars);
        }

        private void InitializePackButtons()
        {
            var currentPackParams = _playerProgressService.GetPackParams();
            var index = 0;
            
            HorizontalLayoutGroup horizontalLayoutGroup = null;
            currentPackParams.ForEach(packParams =>
            {
                if (horizontalLayoutGroup == null|| index % 2 == 0)
                {
                    horizontalLayoutGroup = Instantiate(_horizontalLayoutGroupPrefab, _levelEnterPopupsParentTransform);
                    _horizontalGroups.Add(horizontalLayoutGroup);
                }
                
                var enterButton = Instantiate(_packItemWidgetPrefab, horizontalLayoutGroup.transform);
                var isUnlocked = _playerProgressService.IsPackAvailable(packParams.PackNumber);
                var starsRequired = _playerProgressService.GetPackStarsToUnlock(packParams.PackNumber);
                enterButton.Initialize(_levelsParamsStorageData.GetPackParamsData(packParams.PackNumber).PackName, _levelsParamsStorageData.GetPackParamsData(packParams.PackNumber).PackImagePrefab, packParams.PackNumber, isUnlocked,
                    () => TryOpenPack(isUnlocked, packParams), OnUnavailablePackClicked, starsRequired);
                index++;
            });
            
            void OnUnavailablePackClicked()
            {
                _starsDisplayWidget.Bump();
            }

            void TryOpenPack(bool isUnlocked, PackParamsData packParams)
            {
                if (!isUnlocked)
                    return;
                        
                _playerProgressService.CurrentPackNumber = packParams.PackNumber;
                _screenHandler.ShowChooseLevelScreen();
            }
        }
    }
}