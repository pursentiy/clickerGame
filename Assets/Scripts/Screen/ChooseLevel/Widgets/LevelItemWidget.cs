using System;
using Extensions;
using Handlers;
using Installers;
using Services;
using Storage.Levels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Disposable;
using Zenject;

namespace Screen.ChooseLevel.Widgets
{
    public class LevelItemWidget : InjectableMonoBehaviour
    {
        [Inject] private SoundHandler _soundHandler;
        [Inject] private LocalizationService _localization;

        [SerializeField] private Image _lockImage;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _levelDifficultyText;
        [SerializeField] private Image _fadeImage;
        [SerializeField] private Image _levelImage;
        [SerializeField] private Button _levelEnterButton;
        [SerializeField] private Image[] _starsImages;
        [SerializeField] private Material _grayScaleStarMaterial;
        [SerializeField] private Material _defaultStareMaterial;

        public void Initialize(string levelName, Sprite levelSprite, int totalEarnedStars, LevelDifficulty levelDifficulty, bool isUnlocked, Action action)
        {
            var levelKey = $"level_{levelName.ToLower()}";
            _levelText.text = _localization.GetValue(levelKey);
            _levelImage.sprite = levelSprite;
            
            _levelEnterButton.interactable = isUnlocked;
            _lockImage.gameObject.SetActive(!isUnlocked);
            _fadeImage.gameObject.SetActive(!isUnlocked);
            
            SetupStars(totalEarnedStars);
            SetDifficultyText(levelDifficulty);
            
            _levelEnterButton.onClick.MapListenerWithSound(action.SafeInvoke).DisposeWith(this);
        }

        private void SetDifficultyText(LevelDifficulty difficulty)
        {
            var label = _localization.GetValue("difficulty_setup");
            var value = _localization.GetValue($"difficulty_{difficulty.ToString().ToLower()}");
            _levelDifficultyText.text = $"{label}: {value}";
        }
        
        private void SetupStars(int totalEarnedStars)
        {
            for (var i = 0; i < _starsImages.Length; i++)
            {
                _starsImages[i].material = i >= totalEarnedStars ? _grayScaleStarMaterial : _defaultStareMaterial;
            }
        }
    }
}