namespace Pooling
{
    public interface IPoolObject
    {
        void Initialize(PoolType poolType);
        void ResetObject();
    }
}