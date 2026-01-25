using Handlers.UISystem.Popups;
using UnityEngine;

namespace UI.Popups.MessagePopup
{
    public class MessagePopupContext : IPopupContext
    {
        public MessagePopupContext(string text, RectTransform anchor, ClanPopupFacing facing = ClanPopupFacing.Left)
        {
            Text = text;
            Anchor = anchor;
            Facing = facing;
        }

        public MessagePopupContext(string text, RectTransform anchor, Color textColor, ClanPopupFacing facing = ClanPopupFacing.Left)
        {
            Text = text;
            Anchor = anchor;
            Facing = facing;
            TextColor = textColor;
        }

        public string Text { get; }
        public RectTransform Anchor { get; }
        public ClanPopupFacing Facing { get; }
        public Color TextColor { get; }
    }

    public enum ClanPopupFacing
    {
        Right,
        Left
    }
}