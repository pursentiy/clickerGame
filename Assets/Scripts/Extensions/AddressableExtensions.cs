using System;
using System.Collections;
using System.Diagnostics;
using RSG;
using Services;
using Services.CoroutineServices;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utilities;

namespace Extensions
{
    public static class AddressableExtensions
    {
        public static IPromise<Either<T, Exception>> OnResult<T>(
            this AsyncOperationHandle<T> handle,
            IPromise delay,
            ICoroutineService coroutineService,
            float timeout = 0,
            float sendErrorTimeout = 0,
            string debugAssetKey = null)
        {
            var promise = new Promise<Either<T, Exception>>();

            coroutineService.StartCoroutine(ProcessOperation());

            IEnumerator ProcessOperation()
            {
                var previousProgress = 0f;
                var operationStopWatch = Stopwatch.StartNew();
                var noProgressStopWatch = Stopwatch.StartNew();
                var longestTimeWithoutProgress = 0L;
                var assetKeyString =
                    !string.IsNullOrEmpty(debugAssetKey) ? $"AssetKey: {debugAssetKey}, " : string.Empty;
                var errorSent = false;
                
                bool isNativeDone = false;
                handle.Completed += (op) => { isNativeDone = true; };
                
                while (handle.IsValid() && !isNativeDone)
                {
                    var currentProgress = handle.GetDownloadStatus().Percent;
                    if (currentProgress.IsEqualWithEpsilon(previousProgress))
                    {
                        var timeWithoutProgress = noProgressStopWatch.Elapsed.TotalSeconds;
                        if (timeWithoutProgress > longestTimeWithoutProgress)
                            longestTimeWithoutProgress = (long)timeWithoutProgress;
                    }
                    else
                    {
                        previousProgress = currentProgress;
                        noProgressStopWatch.Restart();
                    }

                    var elapsedSeconds = operationStopWatch.Elapsed.TotalSeconds;
                    if (timeout > 0 && elapsedSeconds >= timeout)
                    {
                        promise.SafeResolve(new Either<T, Exception>(new TimeoutException(
                            $"Addressables timeout. {assetKeyString}handle: {handle.DebugName}, elapsed: {elapsedSeconds:F}s")));
                        yield break;
                    }
                    
                    if (sendErrorTimeout > 0 && elapsedSeconds >= sendErrorTimeout && !errorSent)
                    {
                        LoggerService.LogError($"Addressables slow load: {assetKeyString}{handle.DebugName}");
                        errorSent = true;
                    }

                    yield return null;
                }
                
                while (delay.IsPending)
                {
                    yield return null;
                }
                
                var status = handle.IsValid() ? handle.Status : AsyncOperationStatus.Failed;

                if (status == AsyncOperationStatus.Succeeded)
                {
                    promise.SafeResolve(new Either<T, Exception>(handle.Result));
                }
                else
                {
                    var exception = handle.IsValid()
                        ? handle.OperationException
                        : new Exception($"Operation failed or handle invalidated: {debugAssetKey}");
                    promise.SafeResolve(new Either<T, Exception>(exception));
                }
            }

            return promise;
        }
    }
}