using Plugins.FSignal;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Services.ScreenObserver
{
    public class DisplayEventProxy : UIBehaviour
    {
        public readonly FSignal ScreenChangedSignal = new();

        public static DisplayEventProxy Create()
        {
            var go = new GameObject("[DisplayMonitorProxy]");
            DontDestroyOnLoad(go);
            
            go.hideFlags = HideFlags.HideAndDontSave;
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
            var proxy = go.AddComponent<DisplayEventProxy>();
            return proxy;
        }
        
        protected override void OnRectTransformDimensionsChange()
        {
            ScreenChangedSignal.Dispatch();
        }
    }
}