using System;
using Handlers.UISystem.Popups;
using TMPro;

namespace UI.Popups.UniversalPopup
{
    public class UniversalPopupContext : IPopupContext
    {
        public string Title { get; }
        public string Message { get; }
        public UniversalPopupButtonAction[] Buttons { get; }
        public bool AllowBackgroundClose { get; }
        public TMP_SpriteAsset SpriteAsset { get; }

        public UniversalPopupContext(
            string message,
            UniversalPopupButtonAction[] buttons,
            string title = null,
            bool closable = true,
            TMP_SpriteAsset spriteAsset = null)
        {
            Title = title;
            Message = message;
            Buttons = buttons;
            AllowBackgroundClose = closable;
            SpriteAsset = spriteAsset;
        }
    }

    public class UniversalPopupButtonAction
    {
        public string Label { get; }
        public Action Callback { get; }
        public UniversalPopupButtonStyle Style { get; }
    
        public UniversalPopupButtonAction(string label, Action callback, UniversalPopupButtonStyle style = UniversalPopupButtonStyle.Common)
        {
            Label = label;
            Callback = callback;
            Style = style;
        }
    }
}