using Extensions;
using Handlers;
using Services;
using ThirdParty.SuperScrollView.Scripts.List;
using Utilities.Disposable;
using Zenject;

namespace UI.Screens.ChooseLevel.LevelItem
{
    public class LevelItemWidgetMediator : InjectableListItemMediator<LevelItemWidgetView, LevelItemWidgetInfo>
    {
        [Inject] private SoundHandler _soundHandler;
        [Inject] private LocalizationService _localization;
        
        public LevelItemWidgetMediator(LevelItemWidgetInfo data) : base(data)
        {
            
        }

        protected override void OnInitialize(bool isVisibleOnRefresh)
        {
            base.OnInitialize(isVisibleOnRefresh);
            
            SetupButtons();
            SetupTexts();
            SetupImages();
            SetupStars(Data.TotalEarnedStars);
        }

        private void SetupButtons()
        {
            View.LevelEnterButton.interactable = Data.IsUnlocked;
            View.LevelEnterButton.onClick.MapListenerWithSound(Data.StartLevel.SafeInvoke).DisposeWith(this);
        }

        private void SetupTexts()
        {
            var label = _localization.GetValue("difficulty_setup");
            var value = _localization.GetValue($"difficulty_{Data.LevelDifficulty.ToString().ToLower()}");
            View.LevelDifficultyText.SetText($"{label}: {value}");
            
            var levelKey = $"level_{Data.LevelName.ToLower()}";
            View.LevelText.SetText(_localization.GetValue(levelKey));
        }

        private void SetupImages()
        {
            View.LevelImage.sprite = Data.LevelSprite;
            
            View.LockImage.gameObject.SetActive(!Data.IsUnlocked);
            View.FadeImage.gameObject.SetActive(!Data.IsUnlocked);
        }
        
        private void SetupStars(int totalEarnedStars)
        {
            var index = 0;

            foreach (var star in View.StarsImages)
            {
                if (star == null)
                {
                    index++;
                    continue;
                }

                star.material = index >= totalEarnedStars ? View.GrayScaleStarMaterial : View.DefaultStareMaterial;
                index++;
            }
        }
    }
}