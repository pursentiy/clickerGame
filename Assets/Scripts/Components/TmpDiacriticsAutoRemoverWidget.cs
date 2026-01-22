using Extensions;
using Installers;
using Services;
using TMPro;
using UnityEngine;
using Utilities.Disposable;
using Zenject;

namespace Components
{
    //Put on the object only with automatic text replacement
    [RequireComponent(typeof(TMP_Text))]
    public class TmpDiacriticsAutoRemoverWidget : InjectableMonoBehaviour
    {
        [Inject] private readonly GameSettingsManager _gameSettingsManager;
        
        [SerializeField] private TMP_Text _text;

        protected override void Awake()
        {
            base.Awake();
            
            TrySetTmpText();
            
            //TODO ADD LOGIC FOR FIXING WELCOME SCREEN IN SPANISH
            _gameSettingsManager.OnLanguageChangedSignal.MapListener(TrySetTmpText).DisposeWith(this);
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