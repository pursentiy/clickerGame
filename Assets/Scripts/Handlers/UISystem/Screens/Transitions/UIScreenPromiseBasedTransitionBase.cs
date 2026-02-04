using System;
using RSG;

namespace Handlers.UISystem.Screens.Transitions
{
    public abstract class UIScreenPromiseBasedTransitionBase : UIScreenTransition
    {
        protected UIScreenPromiseBasedTransitionBase(Type toScreenType, IScreenContext context,
            bool downloadPrefabWithProgress) : base(toScreenType, context, downloadPrefabWithProgress)
        {
        }

        public abstract IPromise DoTransition(UIScreenBase fromScreen, UIScreenBase toScreen, bool forward);
    }
}