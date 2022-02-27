using System;
using Handlers;
using Installers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Random = UnityEngine.Random;

namespace Screen.SubElements
{
    public class PackEnterWidgetHandler : InjectableMonoBehaviour
    {
        
        [Inject] private SoundHandler _soundHandler;
        
        [SerializeField] private Image _lockImage;
        [SerializeField] private Image _fadeImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _packImage;
        [SerializeField] private Sprite[] _backgroundSpritesArray;
        [SerializeField] private TMP_Text _packText;
        [SerializeField] private Button _packEnterButton;

        private int _currentPackNumber;
        public void Initialize(string packName, Sprite packSprite, int packNumber, bool isUnlocked, Action action)
        {
            _packText.text = packName;

            _packImage.sprite = packSprite;

            _currentPackNumber = packNumber;

            _packEnterButton.interactable = isUnlocked;
            _lockImage.gameObject.SetActive(!isUnlocked);
            _fadeImage.gameObject.SetActive(!isUnlocked);

            _backgroundImage.sprite = _backgroundSpritesArray[Random.Range(0, _backgroundSpritesArray.Length - 1)];

            _packEnterButton.onClick.AddListener(()=>
            {
                _soundHandler.PlayButtonSound();
                action.Invoke();
            });
        }

        private void OnDestroy()
        {
            _packEnterButton.onClick.RemoveAllListeners();
        }
    }
}