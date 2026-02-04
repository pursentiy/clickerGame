using System;
using Extensions;
using Installers;
using UnityEngine;

namespace Handlers.UISystem.Screens.Transitions
{
    public class InstantScreenTransition : UIScreenTimeBasedTransitionBase
    {
        
        public InstantScreenTransition(Type toScreenType, IScreenContext context) : base(toScreenType, context, false)
        {
            ContainerHolder.CurrentContainer.Inject(this);
        }

        public override void Prepare(UIScreenBase fromScreenInstance, UIScreenBase toScreenInstance, bool forward)
        {
            toScreenInstance.GetComponent<CanvasGroup>().alpha = 1;
            
            fromScreenInstance.GetRectTransform().anchoredPosition = Vector2.zero;
            toScreenInstance.GetRectTransform().anchoredPosition = Vector2.zero;
        }

        public override void DoTransition(float t, bool forward)
        {
        }

        public override void OnComplete(UIScreenBase fromScreenInstance, UIScreenBase toScreenInstance, bool forward)
        {
            
        }

        public override void OnTransitionEnded(bool forward)
        {
            base.OnTransitionEnded(forward);
        }

        public override float TransitionTime => 0.5f;
    }
}