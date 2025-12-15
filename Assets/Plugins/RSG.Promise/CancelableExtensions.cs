using Plugins.RSG.Promise;

namespace Extensions
{
    public static class CancelableExtensions
    {
        public static ICancellablePromise FindLastPendingParent(this ICancellablePromise cancellable)
        {
            ICancellablePromise result;
            var next = cancellable;
            
            do
            {
                result = next;
                next = result.Parent;
            } while (next != null && next.CanBeCanceled);

            return result;
        }
    }
}