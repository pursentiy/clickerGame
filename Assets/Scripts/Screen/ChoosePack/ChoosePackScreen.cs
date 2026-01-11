using System.Collections.Generic;
using Common.Widgets.Animations;
using Components.UI;
using Extensions;
using Handlers;
using Handlers.UISystem;
using Popup.Settings;
using Screen.ChoosePack.Widgets;
using Services;
using Storage;
using Storage.Levels;
using TMPro;
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
        [Inject] private LocalizationService _localization;
        [Inject] private UIManager _uiManager;
        
        [SerializeField] private PackItemWidget _packItemWidgetPrefab;
        [SerializeField] private RectTransform _levelEnterPopupsParentTransform;
        [SerializeField] private HorizontalLayoutGroup _horizontalLayoutGroupPrefab;
        [SerializeField] private CurrencyDisplayWidget _starsDisplayWidget;
        [SerializeField] private TextMeshProUGUI _headerText;
        [SerializeField] private Button _goBack;
        [SerializeField] private Button _settingsButton;

        private List<HorizontalLayoutGroup> _horizontalGroups = new();

        protected override void Start()
        {
            base.Start();
            
            _headerText.text = _localization.GetGameValue("choose_pack_header");

            InitializePackButtons();
            _starsDisplayWidget.SetCurrency(_playerCurrencyService.Stars);
            
            _goBack.onClick.MapListenerWithSound(()=> _screenHandler.ShowWelcomeScreen());
            _settingsButton.onClick.MapListenerWithSound(()=> _uiManager.PopupsHandler.ShowPopupImmediately<SettingsPopupMediator>(null));
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