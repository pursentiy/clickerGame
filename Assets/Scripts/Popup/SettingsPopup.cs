using Handlers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Popup
{
    public class SettingsPopup : PopupBase
    {
        [Inject] private SoundHandler _soundHandler;
        
        [SerializeField] private Button _closeButton;
        [SerializeField] private Toggle _musicToggle;
        [SerializeField] private Toggle _soundToggle;

        protected override void OnCreated()
        {
            base.OnCreated();

            _closeButton.onClick.AddListener(()=>
            {
                _soundHandler.PlayButtonSound();
                _popupHandler.HideCurrentPopup();
            });
        }

        private void OnDestroy()
        {
            _closeButton.onClick.RemoveAllListeners();
        }
    }
}