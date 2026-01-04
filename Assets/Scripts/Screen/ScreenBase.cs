using Installers;
using UnityEngine;

namespace Screen
{
    public abstract class ScreenBase : InjectableMonoBehaviour
    {
        [SerializeField] private GameObject _backgroundGameObjectPrefab;

        GameObject _backgroundGameObject;
        
        protected override void Awake()
        {
            base.Awake();
            
            _backgroundGameObject = ContainerHolder.CurrentContainer.InstantiatePrefab(_backgroundGameObjectPrefab);
        }

        private void OnDestroy()
        {
            if (_backgroundGameObject != null)
                Destroy(_backgroundGameObject);
        }
    }
}