using System.Collections.Generic;
using Common.Widgets.Animations;
using Components.UI;
using Extensions;
using Handlers;
using Handlers.UISystem;
using Popup.Settings;
using Popup.Universal;
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
        [Inject] private readonly ScreenHandler _screenHandler;
        [Inject] private readonly PlayerProgressService _playerProgressService;
        [Inject] private readonly PlayerService _playerService;
        [Inject] private readonly LevelsParamsStorageData _levelsParamsStorageData;
        [Inject] private readonly PlayerCurrencyService _playerCurrencyService;
        [Inject] private readonly LocalizationService _localization;
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly LocalizationService _localizationService;
        
        [SerializeField] private PackItemWidget _packItemWidgetPrefab;
        [SerializeField] private RectTransform _levelEnterPopupsParentTransform;
        [SerializeField] private HorizontalLayoutGroup _horizontalLayoutGroupPrefab;
        [SerializeField] private CurrencyDisplayWidget _starsDisplayWidget;
        [SerializeField] private TextMeshProUGUI _headerText;
        [SerializeField] private TextMeshProUGUI _availablePacksText;
        [SerializeField] private Button _goBack;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _infoButton;

        private List<HorizontalLayoutGroup> _horizontalGroups = new();

        protected override void Start()
        {
            base.Start();
            
            _headerText.text = _localization.GetGameValue("choose_pack_header");

            InitializePackButtons();
            SetAvailablePacksText();
            _starsDisplayWidget.SetCurrency(_playerCurrencyService.Stars);
            
            _infoButton.onClick.MapListenerWithSound(OnInfoButtonClicked);
            _goBack.onClick.MapListenerWithSound(()=> _screenHandler.ShowWelcomeScreen());
            _settingsButton.onClick.MapListenerWithSound(()=> _uiManager.PopupsHandler.ShowPopupImmediately<SettingsPopupMediator>(null));
        }
        
        private void OnInfoButtonClicked()
        {
            var context = new UniversalPopupContext(
                _localizationService.GetCommonValue("unlock_sets_info"),
                new[] {
                    new UniversalPopupButtonAction(_localizationService.GetCommonValue(LocalizationExtensions.OkKey), null)
                }, _localizationService.GetCommonValue(LocalizationExtensions.InfoTitle));

            _uiManager.PopupsHandler.ShowPopupImmediately<UniversalPopupMediator>(context);
        }

        private void SetAvailablePacksText()
        {
            var totalPacks = _playerProgressService.GetAllPacksCount();
            var totalAvailablePacks = _playerProgressService.GetAllAvailablePacksCount();
            
            _availablePacksText.text = _localization.GetFormattedCommonValue("unlocked_sets", $"{totalAvailablePacks}/{totalPacks}");
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
        
        private void OnDestroy()
        {
            _uiManager.PopupsHandler.HideAllPopups<UniversalPopupMediator>();
        }
    }
}