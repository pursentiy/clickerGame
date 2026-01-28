using System;

namespace Handlers.UISystem.Screens.Transitions
{
    public abstract class UIScreenTimeBasedTransitionBase : UIScreenTransition
    {
        protected UIScreenTimeBasedTransitionBase(Type toScreenType, IScreenContext context,
            bool downloadPrefabWithProgress) : base(toScreenType, context, downloadPrefabWithProgress)
        {
        }

        public abstract float TransitionTime { get; }
        public bool SlowDown { get; protected set; } = false;

        public abstract void Prepare(UIScreenBase fromScreenInstance, UIScreenBase toScreenInstance, bool forward);

        public abstract void DoTransition(float t, bool forward);

        public abstract void OnComplete(UIScreenBase fromScreenInstance, UIScreenBase toScreenInstance, bool forward);
    }
}