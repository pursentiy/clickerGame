using UnityEngine;

namespace Installers
{
    public class InjectableMonoBehaviour : MonoBehaviour
    {
        private bool _injected;

        protected virtual void Awake()
        {
            InjectSelf();
        }

        protected void InjectSelf()
        {
            if (_injected)
            {
                return;
            }

            _injected = true;
            ContainerHolder.CurrentContainer.Inject(this);
        }
    }
}