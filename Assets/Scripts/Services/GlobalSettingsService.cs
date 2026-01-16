using Services.Base;
using UnityEngine;

namespace Services
{
    public class GlobalSettingsService : DisposableService
    {
        protected override void OnInitialize()
        {
            Input.multiTouchEnabled = false;
        }

        protected override void OnDisposing()
        {
            
        }
    }
}