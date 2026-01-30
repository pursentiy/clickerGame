using Installers;
using Platform.Common.Components.List;

namespace Plugins.SuperScrollView.Scripts.List
{
    public abstract class InjectableListItemMediator<TView, TData> : ListItemMediator<TView, TData> where TView : ItemViewBase
    {
        protected InjectableListItemMediator(TData data) : base(data)
        {
        }
    
        protected override void OnInitialize(bool isVisibleOnRefresh)
        {
            ContainerHolder.CurrentContainer.Inject(this);
            base.OnInitialize(isVisibleOnRefresh);
        }
    }
}