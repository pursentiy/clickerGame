using System;
using Plugins.FSignal;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Screen.SubElements
{
    public class LevelEnterWidgetHandler : MonoBehaviour
    {
        [SerializeField] private Image _lockImage;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private Image _fadeImage;
        [SerializeField] private Image _levelImage;
        [SerializeField] private Button _levelEnterButton;
        [SerializeField] private Image[] _starsImages;

        public void Initialize(string levelName, Sprite levelSprite, int difficulty, bool isUnlocked, Action action)
        {
            _levelText.text = levelName;
            
            _levelEnterButton.interactable = isUnlocked;
            _lockImage.gameObject.SetActive(!isUnlocked);
            _fadeImage.gameObject.SetActive(!isUnlocked);
            
            _levelImage.sprite = levelSprite;

            for (var i = 0; i < _starsImages.Length; i++)
            {
                _starsImages[i].gameObject.SetActive(i < difficulty);
            }

            _levelEnterButton.onClick.AddListener(action.Invoke);
        }

        private void OnDestroy()
        {
            _levelEnterButton.onClick.RemoveAllListeners();
        }
    }
}