using Attributes;
using Extensions;
using Handlers.UISystem;
using UI.Popups.CommonPopup;
using UnityEngine;
using Utilities.Disposable;

namespace UI.Popups.MessagePopup
{
    [AssetKey("UI Popups/MessagePopupMediator")]
    public class MessagePopupMediator : UIPopupBase<MessagePopupView, MessagePopupContext>
    {
        public override IUIPopupAnimation Animation => new ScalePopupAnimation(View.PopupTransform);

        private void ApplyContextFacing()
        {
            var bubbleAnchor = View.BubbleTransform.anchoredPosition;
            var bubblePivot = View.BubbleTransform.pivot;
            var cornerAnchor = View.CornerTransform.anchoredPosition;
            switch (Context.Facing)
            {
                case PopupFacing.Right:
                    bubbleAnchor = new Vector2(-1f * bubbleAnchor.x, bubbleAnchor.y);
                    bubblePivot = new Vector2(1f - bubblePivot.x, bubblePivot.y);
                    cornerAnchor = new Vector2(-1f * cornerAnchor.x, cornerAnchor.y);
                    break;
            }
            View.BubbleTransform.anchoredPosition = bubbleAnchor;
            View.BubbleTransform.pivot = bubblePivot;
            View.CornerTransform.anchoredPosition = cornerAnchor;
        }

        public override void OnCreated()
        {
            base.OnCreated();
            
            View.PopupTransform.position = Context.Anchor.position;
            ApplyContextFacing();

            View.Text.text = Context.Text;
            View.Text.spriteAsset = Context.SpriteAsset;

            View.BackgroundCloseButton.onClick.MapListenerWithSound(Hide).DisposeWith(this);
        }
    }
}