using Utilities;

namespace Services.Base
{
    public interface IDisposableService : IDisposeProvider
    {
        void InitializeService();
        void DisposeService();
    }
}