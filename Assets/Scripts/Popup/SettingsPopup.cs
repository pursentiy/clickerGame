using UnityEngine;
using UnityEngine.UI;

namespace Popup
{
    public class SettingsPopup : PopupBase
    {
        [SerializeField] private Button _closeButton;

        protected override void OnCreated()
        {
            base.OnCreated();
            
            _closeButton.onClick.AddListener(()=> _popupHandler.HideCurrentPopup());
        }

        private void OnDestroy()
        {
            _closeButton.onClick.RemoveAllListeners();
        }
    }
}