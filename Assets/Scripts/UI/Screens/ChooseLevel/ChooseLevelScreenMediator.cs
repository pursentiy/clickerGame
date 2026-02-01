using Attributes;
using Controllers;
using Extensions;
using Handlers.UISystem.Screens;
using Services;
using Services.Player;
using Services.ScreenObserver;
using UI.Popups.MessagePopup;
using UnityEngine.UI;
using Utilities.Disposable;
using Zenject;

namespace UI.Screens.ChooseLevel
{
    [AssetKey("UI Screens/ChooseLevelScreenMediator")]
    public class ChooseLevelScreenMediator : UIScreenBase<ChooseLevelScreenView, ChooseLevelScreenContext>
    {
        [Inject] private readonly ProgressProvider _progressProvider;
        [Inject] private readonly ProgressController _progressController;
        [Inject] private readonly PlayerCurrencyService _playerCurrencyService;
        [Inject] private readonly LocalizationService _localizationService;
        [Inject] private readonly ScreenObserverService _screenObserverService;
        [Inject] private readonly FlowPopupController _flowPopupController;
        [Inject] private readonly FlowScreenController _flowScreenController;
        
        private HorizontalLayoutGroup _horizontalGroup;
        private int _currentPackId;

        public override void OnCreated()
        {
            base.OnCreated();
            
            _currentPackId = _progressController.CurrentPackId;

            InitWidgets();
            SetTexts();

            View.StarsDisplayWidget.SetCurrency(_playerCurrencyService.Stars);
            
            View.GoBackButton.onClick.MapListenerWithSound(OnGoBackButtonClicked).DisposeWith(this);
            View.InfoButton.onClick.MapListenerWithSound(OnInfoButtonClicked).DisposeWith(this);
            View.SettingsButton.onClick.MapListenerWithSound(OnSettingsButtonClicked).DisposeWith(this);
        }

        private void InitWidgets()
        {
            View.LevelsWidget.Initialize(Context.PackInfo.PackId, Context.PackInfo.LevelsInfo);
        }

        private void OnSettingsButtonClicked()
        {
            _flowPopupController.ShowSettingsPopup(true);
        }

        private void OnGoBackButtonClicked()
        {
            _flowScreenController.GoToChoosePackScreen();
        }

        private void SetTexts()
        {
            View.HeaderText.SetText(_localizationService.GetValue("choose_level_header"));
            
            var localizedName = _localizationService.GetValue($"pack_{Context.PackInfo.PackName.ToLower()}");
            var wordPack = _localizationService.GetValue("word_pack");
            View.PackName.SetText($"{localizedName} {wordPack}");
            
            var totalLevels = _progressProvider.GetLevelsCountInPack(_currentPackId);
            var totalAvailableLevels = _progressProvider.GetLevelsCountInPack(_currentPackId, true);
            View.AvailableLevelsText.SetText(_localizationService.GetFormattedValue("unlocked_levels", $"{totalAvailableLevels}/{totalLevels}"));
        }

        private void OnInfoButtonClicked()
        {
            var context = new MessagePopupContext(_localizationService.GetValue("unlock_sets_info"), View.InfoButton.GetRectTransform(), View.InfoMessageFontSize, facing: PopupFacing.Right);
            _flowPopupController.ShowMessagePopup(context, overrideDisposeProvider: this.GetDisposeProvider());
        }

        // private IEnumerator InitializeLevelsRoutine()
        // {
        //     var index = 0;
        //     _horizontalGroup = null;
        //
        //     foreach (var levelParams in Context.PackInfo.LevelsInfo)
        //     {
        //         if (this == null || gameObject == null)
        //             yield break;
        //
        //         if (_horizontalGroup == null || index % 2 == 0)
        //         {
        //             _horizontalGroup = Instantiate(View._horizontalLayoutGroupPrefab, View._levelEnterPopupsParentTransform);
        //         }
        //
        //         var enterButton = Instantiate(View._levelItemWidgetPrefab, _horizontalGroup.transform);
        //         var earnedStarsForLevel = _progressProvider.GetEarnedStarsForLevel(_currentPackId, levelParams.LevelId) ?? 0;
        //
        //         enterButton.Initialize(
        //             levelParams.LevelName, 
        //             levelParams.LevelImage, 
        //             earnedStarsForLevel, 
        //             levelParams.LevelDifficulty, 
        //             _progressProvider.IsLevelAvailableToPlay(_currentPackId, levelParams.LevelId),
        //             () => StartLevel(Context.PackInfo, levelParams)
        //         );
        //
        //         index++;
        //         
        //         yield return new WaitForSecondsRealtime(0.05f);
        //     }
        // }
    }
}