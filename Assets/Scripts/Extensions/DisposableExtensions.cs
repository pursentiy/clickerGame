using Utilities;

namespace Extensions
{
    public static class DisposableExtensions
    {
        public static void HandledDispose(IDisposeProvider provider)
        {
            provider.ChildDisposables.Dispose();
        }
    }
}