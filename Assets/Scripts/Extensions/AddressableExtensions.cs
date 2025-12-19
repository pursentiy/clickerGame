using System;
using System.Collections;
using System.Diagnostics;
using RSG;
using Services;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utilities;

namespace Extensions
{
    public static class AddressableExtensions
    {
            public static IPromise<Either<T, Exception>> OnResult<T>(
        this AsyncOperationHandle<T> handle, 
        IPromise delay, 
        CoroutineService coroutineService, 
        float timeout = 0, 
        float sendErrorTimeout = 0,
        string debugAssetKey = null)
    {
        var promise = new Promise<Either<T, Exception>>();
        
        coroutineService.StartCoroutine(ProcessOperation());
        
        IEnumerator ProcessOperation()
        {
            var previousProgress = 0f;
            var operationStopWatch = new Stopwatch();
            operationStopWatch.Start();
            var noProgressStopWatch = new Stopwatch();
            noProgressStopWatch.Start();
            var longestTimeWithoutProgress = 0L;
        
            var assetKeyString = !debugAssetKey.IsNullOrEmpty() ? $"AssetKey: {debugAssetKey}, " : string.Empty;
            
            var errorSent = false;

            while (handle.IsValid() && (!handle.IsDone || delay.IsPending))
            {
                var currentProgress = handle.GetDownloadStatus().Percent;

                if (currentProgress.IsEqualWithEpsilon(previousProgress))
                {
                    var timeWithoutProgress = noProgressStopWatch.Elapsed.TotalSeconds;
                    
                    if (timeWithoutProgress > longestTimeWithoutProgress)
                    {
                        longestTimeWithoutProgress = (long)timeWithoutProgress;
                    }
                }
                else
                {
                    previousProgress = currentProgress;
                    noProgressStopWatch.Reset();
                    noProgressStopWatch.Start();
                }

                yield return null;
                
                var elapsedSeconds = operationStopWatch.Elapsed.TotalSeconds;
                
                if (timeout > 0 && elapsedSeconds >= timeout)
                {
                    promise.SafeResolve(new Either<T, Exception>(new TimeoutException(
                        $"Addressables timeout. {assetKeyString}handle name: {handle.DebugName ?? "n/a"}, delay state: {delay.CurState}, " +
                        $"longestTimeWithoutProgress: {longestTimeWithoutProgress:F}s, operation elapsed time: {operationStopWatch.Elapsed.TotalSeconds:F}s")));
                    yield break;
                }

                if (sendErrorTimeout > 0 && elapsedSeconds >= sendErrorTimeout && !errorSent)
                {
                    LoggerService.LogError($"Addressables sendErrorTimeout. {assetKeyString}handle name: {handle.DebugName ?? "n/a"}, delay state: {delay.CurState}, " +
                                           $"longestTimeWithoutProgress: {longestTimeWithoutProgress:F}s, operation elapsed time: {operationStopWatch.Elapsed.TotalSeconds:F}s");

                    errorSent = true;
                }
            }
            
            var status = handle.IsValid() ? handle.Status : AsyncOperationStatus.Failed;
            
            switch (status)
            {
                case AsyncOperationStatus.Succeeded:
                    promise.SafeResolve(new Either<T, Exception>(handle.Result));
                    break;
                case AsyncOperationStatus.Failed:
                    var exception = handle.IsValid() ? handle.OperationException 
                        : new Exception("Operation handler is not valid");
                    promise.SafeResolve(new Either<T, Exception>(exception));
                    break;
            }
        }

        return promise;
    }
    }
}