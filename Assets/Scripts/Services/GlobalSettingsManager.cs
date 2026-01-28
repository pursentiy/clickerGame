using UnityEngine;

namespace Services
{
    public class GlobalSettingsManager
    {
        public void DisableMultitouch()
        {
            Input.multiTouchEnabled = false;
        }

        public void SetMaxFrameRate()
        {
            Application.targetFrameRate = 60;
        }
    }
}