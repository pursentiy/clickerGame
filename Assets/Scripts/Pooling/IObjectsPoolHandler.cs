using UnityEngine;

namespace Pooling
{
    public interface IObjectsPoolHandler
    {
        GameObject GetPoolPrefab(PoolType poolType);
        void ResetPoolObjectParent(PoolObject poolObject, PoolType poolType);
        void TryRemoveOldComponents(GameObject poolObj);
    }
}