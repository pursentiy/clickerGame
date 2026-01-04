using Extensions;
using UnityEngine;

namespace Common.Widgets
{
    public class BackgroundController : MonoBehaviour
    {
        [SerializeField] SunController _sunController;
        [SerializeField] WorldShadowController[] _shadowControllers;

        private void Update()
        {
            TryUpdateShadowControllers();
        }

        private void TryUpdateShadowControllers()
        {
            if (_shadowControllers.IsNullOrEmpty())
                return;

            var sunPosition = _sunController.SunPosition;
            foreach (var shadowController in _shadowControllers)
            {
                shadowController.UpdateShadows(sunPosition);
            }
        }
    }
}