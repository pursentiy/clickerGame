using Handlers.UISystem.Popups;
using TMPro;
using UnityEngine;

namespace UI.Popups.MessagePopup
{
    public class MessagePopupContext : IPopupContext
    {
        public MessagePopupContext(string text, RectTransform anchor, float fontSize = 100, TMP_SpriteAsset spriteAsset = null, PopupFacing facing = PopupFacing.Left)
        {
            Text = text;
            Anchor = anchor;
            FontSize = fontSize;
            SpriteAsset = spriteAsset;
            Facing = facing;
        }

        public string Text { get; }
        public RectTransform Anchor { get; }
        public PopupFacing Facing { get; }
        public float FontSize { get; }
        public TMP_SpriteAsset SpriteAsset { get; }
    }

    public enum PopupFacing
    {
        Right,
        Left
    }
}