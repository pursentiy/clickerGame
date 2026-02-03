using DG.Tweening;
using Extensions;
using RSG;
using ThirdParty.SuperScrollView.Scripts.List;
using UnityEngine;
using Utilities.Disposable;

namespace UI.Screens.PuzzleAssembly.Widgets.PuzzleItem
{
    public class PuzzleMenuItemWidgetMediator : InjectableListItemMediator<PuzzleMenuItemWidgetView, PuzzleMenuItemWidgetInfo>
    {
        private Sequence _fadeAnimationSequence;
        
        public bool IsCompleted { get; private set; }
        
        public PuzzleMenuItemWidgetMediator(PuzzleMenuItemWidgetInfo data) : base(data)
        {
        }

        protected override void OnInitialize(bool isVisibleOnRefresh)
        {
            base.OnInitialize(isVisibleOnRefresh);

        }
        
        public IPromise SetConnected()
        {
            IsCompleted = true;
            
            return FadeFigure();
        }
        
        private IPromise FadeFigure()
        {
            var color = View.Image.color;

            _fadeAnimationSequence = DOTween.Sequence()
                .Append(View.Image.DOColor(new Color(color.r, color.g, color.b, 0.5f), 0.2f))
                .KillWith(this);

            return _fadeAnimationSequence.AsPromise();
        }
    }
}