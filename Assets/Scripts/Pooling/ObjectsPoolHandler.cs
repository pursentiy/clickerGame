using System.Collections.Generic;
using UnityEngine;

namespace Pooling
{
    public class ObjectsPoolHandler : MonoBehaviour, IObjectsPoolHandler
    {
        [SerializeField] private Transform _poolObjectsParentTransform;
        [SerializeField] private RectTransform _canvasFieldTransform;
        [SerializeField] private PoolObject _canvasPrefab;
        [SerializeField] private PoolObject _gamePrefab;

        private Dictionary<PoolType, Queue<PoolObject>> _poolDictionary;
        private const int PoolItemsNumber = 50;

        private void Awake()
        {
            SetupDictionary();
        }

        private void SetupDictionary()
        {
            _poolDictionary = new Dictionary<PoolType, Queue<PoolObject>>();

            InstantiateObjects(PoolItemsNumber, PoolType.Game, _gamePrefab);
            InstantiateObjects(PoolItemsNumber, PoolType.Canvas, _canvasPrefab);
        }

        private void InstantiateObjects(int numberOfObjects, PoolType type, PoolObject prefab)
        {
            var objectsQueue = new Queue<PoolObject>();
            for (var i = 0; i < numberOfObjects; i++)
            {
                var obj = Instantiate(prefab, type == PoolType.Game ? _poolObjectsParentTransform : _canvasFieldTransform);
                obj.Initialize(type);
                obj.gameObject.SetActive(false);
                objectsQueue.Enqueue(obj);
            }

            _poolDictionary.Add(type, objectsQueue);
        }

        public GameObject GetPoolPrefab(PoolType poolType)
        {
            if (!_poolDictionary.ContainsKey(poolType))
            {
                Debug.LogWarning($"Pool does not contain key {poolType}");
                return null;
            }

            var obj = _poolDictionary[poolType].Dequeue();
            obj.gameObject.SetActive(true);
            _poolDictionary[poolType].Enqueue(obj);
            return obj.gameObject;
        }

        public void TryRemoveOldComponents(GameObject poolObj)
        {
            var componentList = poolObj.GetComponents(typeof(Component));

            foreach (var component in componentList)
            {
                if (component is RectTransform || component is CanvasRenderer || component is PoolObject || component is Transform)
                {
                }
                else
                {
                    Destroy(component);
                }
            }
        }

        private void OnDestroy()
        {
            _canvasFieldTransform = null;
            _poolObjectsParentTransform = null;
        }

        public void ResetPoolObjectParent(PoolObject poolObject, PoolType poolType)
        {
            if (_canvasFieldTransform == null || _poolObjectsParentTransform == null)
            {
                return;
            }
            
            switch (poolType)
            {
                case PoolType.Canvas when !(_canvasFieldTransform == null):
                    poolObject.transform.SetParent(_canvasFieldTransform);
                    break;
                case PoolType.Game when !(_poolObjectsParentTransform == null):
                    poolObject.transform.SetParent(_poolObjectsParentTransform);
                    break;
            }
        }
        
    }
}

    public enum PoolType
    {
        Game,
        Canvas
    }
