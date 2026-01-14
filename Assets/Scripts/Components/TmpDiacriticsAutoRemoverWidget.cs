using Extensions;
using Services;
using TMPro;
using UnityEngine;

namespace Components
{
    //Put on the object only with automatic text replacement
    [RequireComponent(typeof(TMP_Text))]
    public class TmpDiacriticsAutoRemoverWidget : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;

        private void Awake()
        {
            TrySetTmpText();
        }

        private void Start()
        {
            TryRemoveDiacritics();
        }

        private void TrySetTmpText()
        {
            if (_text != null)
                return;
            
            _text = GetComponent<TMP_Text>();
            LoggerService.LogDebugEditor($"Setting {nameof(TMP_Text)} for {GetType().Name}");
        }

        private void TryRemoveDiacritics()
        {
            if (_text == null || _text.text.IsNullOrEmpty())
                return;

            var clearedText = TmpExtensions.RemoveDiacritics(_text.text);
            if (clearedText.IsNullOrEmpty())
                return;
            
            _text.text = clearedText;
        }
    }
}