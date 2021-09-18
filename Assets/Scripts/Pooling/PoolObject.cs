using Installers;
using Zenject;

namespace Pooling
{
    public class PoolObject : InjectableMonoBehaviour
    {
        [Inject] private ObjectsPoolHandler _objectsPoolHandler;
        
        private PoolType _poolType;

        public void Initialize(PoolType poolType)
        {
            _poolType = poolType;
        }

        public void ResetObject()
        {
            _objectsPoolHandler.ResetPoolObjectParent(this, _poolType);
            _objectsPoolHandler.TryRemoveOldComponents(gameObject);
            gameObject.SetActive(false);
        }
    }
}