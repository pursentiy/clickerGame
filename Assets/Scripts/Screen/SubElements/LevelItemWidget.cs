using System;
using Extensions;
using Handlers;
using Installers;
using Storage.Levels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Disposable;
using Zenject;

namespace Screen.SubElements
{
    public class LevelItemWidget : InjectableMonoBehaviour
    {
        [Inject] private SoundHandler _soundHandler;
        
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
            _levelText.text = levelName;
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
            //TODO LOCALIZATION
            _levelDifficultyText.text = $"Difficulty: {difficulty}";
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