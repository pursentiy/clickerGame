using System;
using Plugins.FSignal;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Screen.SubElements
{
    public class LevelEnterPopupHandler : MonoBehaviour
    {
        [SerializeField] private Image _lockImage;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private Button _levelEnterButton;

        public void Initialize(int levelNumber, bool isUnlocked, Action action)
        {
            _levelText.text = "Level " + levelNumber;

            _levelEnterButton.interactable = isUnlocked;
            _lockImage.gameObject.SetActive(!isUnlocked);

            _levelEnterButton.onClick.AddListener(action.Invoke);
        }

        private void OnDestroy()
        {
            _levelEnterButton.onClick.RemoveAllListeners();
        }
    }
}