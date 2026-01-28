using Services.ContentDeliveryService;
using UnityEngine;

namespace Handlers.UISystem.Screens
{
    public class UIScreen
    {
        public UIScreenBase ScreenBase { get; }
        public IDisposableContent<GameObject> Asset { get; }

        public UIScreen(UIScreenBase screenBase, IDisposableContent<GameObject> asset)
        {
            Asset = asset;
            ScreenBase = screenBase;
        }
    }
}