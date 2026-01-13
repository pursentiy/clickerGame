using System;
using Common.Currency;
using Extensions;
using Handlers;
using Installers;
using Services;
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
        [Inject] private LocalizationService _localization;
        
        [SerializeField] private Image _fadeImage;
        [SerializeField] private RectTransform _packImagePrefabContainer;
        [SerializeField] private TMP_Text _packText;
        [SerializeField] private Button _packEnterButton;
        [SerializeField] private TMP_Text _lockedBlockText;
        [SerializeField] private RectTransform _lockedBlockHolder;
        [SerializeField] private RectTransform _unlockedBlockHolder;

        private GameObject _packImageInstance;
        private int _currentPackNumber;

        public void Initialize(string packName, GameObject packImagePrefab, int packNumber, bool isUnlocked, Action onClickAction, Action onLockedClickAction, Stars starsRequired)
        {
            var packKey = $"pack_{packName.ToLower()}";
            _packText.text = _localization.GetGameValue(packKey);
            _packImageInstance = Instantiate(packImagePrefab, _packImagePrefabContainer);
            _currentPackNumber = packNumber;
            _fadeImage.gameObject.SetActive(!isUnlocked);
            _lockedBlockText.TrySetActive(!isUnlocked);

            _unlockedBlockHolder.gameObject.SetActive(isUnlocked);
            _lockedBlockHolder.gameObject.SetActive(!isUnlocked);

            if (isUnlocked)
            {
                _packEnterButton.onClick.MapListenerWithSound(onClickAction.SafeInvoke).DisposeWith(this);
            }
            else
            {
                _lockedBlockText.text = _localization.GetFormattedCommonValue("pack_stars_required", $"{starsRequired} <sprite=0>");
                _packEnterButton.onClick.MapListenerWithSound(onLockedClickAction.SafeInvoke).DisposeWith(this);
            }
        }
    }
}