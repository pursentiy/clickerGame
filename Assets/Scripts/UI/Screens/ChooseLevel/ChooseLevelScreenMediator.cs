using System.Collections;
using Attributes;
using Common.Data.Info;
using Components.UI;
using Extensions;
using Handlers;
using Handlers.UISystem;
using Handlers.UISystem.Screens;
using Services;
using Services.Player;
using Services.ScreenObserver;
using TMPro;
using UI.Popups.MessagePopup;
using UI.Popups.SettingsPopup;
using UI.Screens.ChooseLevel.Widgets;
using UI.Screens.ChoosePack;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Disposable;
using Zenject;

namespace UI.Screens.ChooseLevel
{
    [AssetKey("UI Screens/ChooseLevelScreenMediator")]
    public class ChooseLevelScreenMediator : UIScreenBase<ChooseLevelScreenView, ChooseLevelScreenContext>
    {
        [Inject] private readonly ScreenHandler _screenHandler;
        [Inject] private readonly ProgressProvider _progressProvider;
        [Inject] private readonly ProgressController _progressController;
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly PlayerCurrencyService _playerCurrencyService;
        [Inject] private readonly LocalizationService _localizationService;
        [Inject] private readonly ScreenObserverService _screenObserverService;
        
        private HorizontalLayoutGroup _horizontalGroup;
        private int _currentPackId;

        public override void OnCreated()
        {
            base.OnCreated();
            
            _currentPackId = _progressController.CurrentPackId;
            
            SetTexts();
            InitializeLevelsButton();
            SetAvailableLevelsText();

            View._starsDisplayWidget.SetCurrency(_playerCurrencyService.Stars);
            
            View._goBack.onClick.MapListenerWithSound(OnGoBackButtonClicked).DisposeWith(this);
            View._infoButton.onClick.MapListenerWithSound(OnInfoButtonClicked).DisposeWith(this);
            View._settingsButton.onClick.MapListenerWithSound(OnSettingsButtonClicked).DisposeWith(this);
            
            _screenObserverService.OnOrientationChangeSignal.MapListener(_ => HideAllInfoMessagesPopups()).DisposeWith(this);
            _screenObserverService.OnResolutionChangeSignal.MapListener(HideAllInfoMessagesPopups).DisposeWith(this);
        }

        private void OnSettingsButtonClicked()
        {
            var context = new SettingsPopupContext(true);
            _uiManager.PopupsHandler.ShowPopupImmediately<SettingsPopupMediator>(context);
        }

        private void OnGoBackButtonClicked()
        {
            _screenHandler.ShowChoosePackScreen();
        }

        private void SetTexts()
        {
            View._headerText.SetText(_localizationService.GetValue("choose_level_header"));
            
            var localizedName = _localizationService.GetValue($"pack_{Context.PackInfo.PackName.ToLower()}");
            var wordPack = _localizationService.GetValue("word_pack");
            View._packName.SetText($"{localizedName} {wordPack}");
        }

        private void OnInfoButtonClicked()
        {
            var fontSize = 150;
            var anchor = View._infoButton.GetRectTransform();
            var context = new MessagePopupContext(_localizationService.GetValue("unlocked_levels_info"), anchor, fontSize);
            _uiManager.PopupsHandler.ShowPopupImmediately<MessagePopupMediator>(context)
                .CancelWith(this);
        }
        
        private void SetAvailableLevelsText()
        {
            var totalLevels = _progressProvider.GetLevelsCountInPack(_currentPackId);
            var totalAvailableLevels = _progressProvider.GetLevelsCountInPack(_currentPackId, true);
            
            View._availableLevelsText.SetText(_localizationService.GetFormattedValue("unlocked_levels", $"{totalAvailableLevels}/{totalLevels}"));
        }

        private void InitializeLevelsButton()
        {
            StartCoroutine(InitializeLevelsRoutine());
        }

        private IEnumerator InitializeLevelsRoutine()
        {
            var index = 0;
            _horizontalGroup = null;

            foreach (var levelParams in Context.PackInfo.LevelsInfo)
            {
                if (this == null || gameObject == null)
                    yield break;

                if (_horizontalGroup == null || index % 2 == 0)
                {
                    _horizontalGroup = Instantiate(View._horizontalLayoutGroupPrefab, View._levelEnterPopupsParentTransform);
                }

                var enterButton = Instantiate(View._levelItemWidgetPrefab, _horizontalGroup.transform);
                var earnedStarsForLevel = _progressProvider.GetEarnedStarsForLevel(_currentPackId, levelParams.LevelId) ?? 0;

                enterButton.Initialize(
                    levelParams.LevelName, 
                    levelParams.LevelImage, 
                    earnedStarsForLevel, 
                    levelParams.LevelDifficulty, 
                    _progressProvider.IsLevelAvailableToPlay(_currentPackId, levelParams.LevelId),
                    () => StartLevel(Context.PackInfo, levelParams)
                );

                index++;
                
                yield return new WaitForSecondsRealtime(0.05f);
            }
        }

        private void StartLevel(PackInfo packInfo, LevelInfo levelInfo)
        {
            if (levelInfo == null)
            {
                LoggerService.LogError($"{nameof(PackInfo)} is null");
                return;
            }

            _screenHandler.StartNewLevel(levelInfo.LevelId, packInfo, levelInfo);
        }

        private void OnDestroy()
        {
            HideAllInfoMessagesPopups();
        }
        
        private void HideAllInfoMessagesPopups()
        {
            _uiManager.PopupsHandler.HideAllPopups<MessagePopupMediator>();
        }
    }
}