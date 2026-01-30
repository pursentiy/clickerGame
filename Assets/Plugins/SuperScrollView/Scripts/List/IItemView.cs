using System;
using Plugins.SuperScrollView.Scripts.ListView;
using SuperScrollView;

namespace Platform.Common.Components.List
{
    public interface IItemView
    {
        event Action<ListItemReleaseType> OnRelease;
        void Release(ListItemReleaseType type = ListItemReleaseType.Default);
        string GetDescription();
    }
}