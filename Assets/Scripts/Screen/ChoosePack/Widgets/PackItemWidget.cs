using System;
using Extensions;
using Handlers;
using Installers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Disposable;
using Zenject;

namespace Screen.ChoosePack.Widgets
{
    public class PackItemWidget : InjectableMonoBehaviour
    {
        
        [Inject] private SoundHandler _soundHandler;
        
        [SerializeField] private Image _lockImage;
        [SerializeField] private Image _fadeImage;
        [SerializeField] private RectTransform _packImagePrefabContainer;
        [SerializeField] private TMP_Text _packText;
        [SerializeField] private Button _packEnterButton;
        [SerializeField] private TMP_Text _lockedBlockText;
        [SerializeField] private RectTransform _lockedBlockHolder;
        [SerializeField] private RectTransform _unlockedBlockHolder;

        private GameObject _packImageInstance;
        private int _currentPackNumber;
        
        public void Initialize(string packName, GameObject packImagePrefab, int packNumber, bool isUnlocked, Action onClickAction, Action onLockedClickAction, int starsRequired)
        {
            _packText.text = packName;
            _packImageInstance = Instantiate(packImagePrefab, _packImagePrefabContainer);
            _currentPackNumber = packNumber;
            _lockImage.gameObject.SetActive(!isUnlocked);
            _fadeImage.gameObject.SetActive(!isUnlocked);
            
            //TODO LOCALIZATION
            _lockedBlockText.text = $"{starsRequired} stars required";

            _unlockedBlockHolder.gameObject.SetActive(isUnlocked);
            _lockedBlockHolder.gameObject.SetActive(!isUnlocked);

            if (isUnlocked)
            {
                
                _packEnterButton.onClick.MapListenerWithSound(onClickAction.SafeInvoke).DisposeWith(this);
            }
            else
            {
                _packEnterButton.onClick.MapListenerWithSound(onLockedClickAction.SafeInvoke).DisposeWith(this);
            }
        }
    }
}