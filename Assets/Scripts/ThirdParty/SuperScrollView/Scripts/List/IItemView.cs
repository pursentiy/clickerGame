using System;
using ThirdParty.SuperScrollView.Scripts.ListView;

namespace ThirdParty.SuperScrollView.Scripts.List
{
    public interface IItemView
    {
        event Action<ListItemReleaseType> OnRelease;
        void Release(ListItemReleaseType type = ListItemReleaseType.Default);
        string GetDescription();
    }
}