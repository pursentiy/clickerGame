using System.Collections.Generic;
using RSG;

namespace Plugins.RSG.Promise
{
    public interface ICancellablePromise
    {
        /// <summary>
        /// Current state of promise
        /// </summary>
        PromiseState CurState { get; }
        
        /// <summary>
        /// Shortcut to check whether a promise is in pending state.
        /// </summary>
        bool CanBeCanceled { get; }
        
        /// <summary>
        /// Promise state shortcut.
        /// </summary>
        bool IsPending { get; }

        /// <summary> 
        /// Add a finally callback. 
        /// Finally callbacks will always be called, even if any preceding promise is cancelled, rejected, or encounters an error.
        /// The returned promise will be resolved, rejected or cancelled as per the preceding promise.
        /// </summary> 
        void Finally(System.Action onComplete);
        
        /// <summary>
        /// A promise that was a source for creation the new one.
        /// </summary>
        ICancellablePromise Parent { get; }
        
        /// <summary>
        /// Promises which were created by applying callbacks to this one.
        /// </summary>
        HashSet<ICancellablePromise> Children { get; }
        
        /// <summary>
        /// Register parent. There can only be one parent.
        /// </summary>
        /// <param name="parent"></param>
        void AttachParent(ICancellablePromise parent);
        
        /// <summary>
        /// Registers a new child.
        /// </summary>
        /// <param name="child"></param>
        void AttachChild(ICancellablePromise child);
        
        /// <summary>
        /// Cancels the whole chain where this promise exists.
        /// </summary>
        void Cancel();
    }
}