using System;
using System.Collections;
using Common;
using Extensions;
using JetBrains.Annotations;
using Platform.Common.Utilities.StateMachine;
using RSG;
using Services.Base;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Services
{
	[UsedImplicitly]
    public class PersistentCoroutinesService : CoroutineServiceBase
	{
		public PersistentCoroutinesService()
		{
			var runner = new GameObject("-PersistentCoroutinesService");
			Object.DontDestroyOnLoad(runner);
			MonoBehaviour = runner.AddComponent<EmptyMonoBehaviour>();
		}
		
		protected override void OnInitialize()
		{
			
		}

		protected override void OnDisposing()
		{
			StopAllCoroutines();
		}
	}
	
	[UsedImplicitly]
	public class CoroutineService : CoroutineServiceBase
	{
		public CoroutineService()
		{
			var runner = new GameObject("-CoroutinesService");
			MonoBehaviour = runner.AddComponent<EmptyMonoBehaviour>();
		}

		protected override void OnInitialize()
		{
			
		}

		protected override void OnDisposing()
		{
			StopAllCoroutines();
		}
	}
	
	[UsedImplicitly]
	public abstract class CoroutineServiceBase : DisposableService
	{
		protected EmptyMonoBehaviour MonoBehaviour;

		public Coroutine StartCoroutine(IEnumerator routine)
		{
			if (routine == null)
			{
				return null;
			}

			if (!MonoBehaviour.Destroyed && MonoBehaviour.gameObject.activeInHierarchy)
			{
				var coroutine = MonoBehaviour.StartCoroutine(routine);
				return coroutine;
			}

			Debug.LogException(
				new Exception("[CoroutineServiceBase] GameObject is not active"));
			
			return null;
		}

		public void StopCoroutine(IEnumerator routine)
		{
			if (routine == null)
			{
				return;
			}

			if (!MonoBehaviour.Destroyed && MonoBehaviour.gameObject.activeInHierarchy)
			{
				MonoBehaviour.StopCoroutine(routine);
			}
		}

		private IEnumerator WaitWhileRoutine(Func<bool> predicate, Promise promise)
		{
			yield return new WaitWhile(predicate);
			promise.Resolve();
		}

		public void StopCoroutine(Coroutine routine)
		{
			if (routine == null)
			{
				return;
			}

			if (!MonoBehaviour.Destroyed && MonoBehaviour.gameObject.activeInHierarchy)
			{
				MonoBehaviour.StopCoroutine(routine);
			}
		}

		public void StopAllCoroutines()
		{
			if (!MonoBehaviour.Destroyed && MonoBehaviour.gameObject.activeInHierarchy)
			{
				MonoBehaviour.StopAllCoroutines();
			}
		}

		public Coroutine WaitFor(float fromS, float toS, Action callback)
		{
			var value = UnityEngine.Random.Range(fromS, toS);
			return WaitFor(value, callback);
		}

		public Coroutine WaitFor(float seconds, Action callback)
		{
			if (!MonoBehaviour.Destroyed && MonoBehaviour.gameObject.activeInHierarchy)
			{
				return MonoBehaviour.StartCoroutine(WaitHandler(seconds, callback));
			}

			Debug.LogException(
				new Exception("[CoroutineServiceBase] GameObject is not active"));
			
			return null;
		}

		public IPromise WaitFrame()
		{
			if (!MonoBehaviour.Destroyed && MonoBehaviour.gameObject.activeInHierarchy)
			{
				var promise = new Promise(this);
				MonoBehaviour.StartCoroutine(WaitFrameHandler(promise));
				return promise;
			}
			
			return Promise.Rejected(new Exception("GameObject is not active"));
		}
		
		public IPromise WaitFrames(int framesCount)
		{
			if (framesCount <= 0)
			{
				return Promise.Resolved();
			}
			
			if (!MonoBehaviour.Destroyed && MonoBehaviour.gameObject.activeInHierarchy)
			{
				var promise = new Promise(this);
				MonoBehaviour.StartCoroutine(WaitFramesHandler(promise, framesCount));
				return promise;
			}
			
			return Promise.Rejected(new Exception("GameObject is not active"));
		}

		private IEnumerator EveryFrameRoutine(Action action)
		{
			while (true)
			{
				action.Invoke();
				yield return new WaitForEndOfFrame();	
			}
		}

		public Coroutine WaitOperation(AsyncOperation operation, Action callback)
		{
			if (!MonoBehaviour.Destroyed && MonoBehaviour.gameObject.activeInHierarchy)
			{
				return MonoBehaviour.StartCoroutine(WaitHandler(operation, callback));
			}

			Debug.LogException(
				new Exception("[CoroutineServiceBase] GameObject is not active"));

			return null;
		}
		
		public IPromise WaitOperation(AsyncOperation operation)
		{
			var promise = new Promise(this);
			
			if (!MonoBehaviour.Destroyed && MonoBehaviour.gameObject.activeInHierarchy)
			{
				var coroutine = MonoBehaviour.StartCoroutine(WaitHandler(operation, () => { promise.SafeResolve(); }));
				promise.OnCanceled(() =>
				{
					if (!MonoBehaviour.Destroyed && MonoBehaviour.gameObject.activeInHierarchy)
						MonoBehaviour.StopCoroutine(coroutine);
				});
			}
			else
			{
				if (promise.IsPending)
				{
					promise.Reject(new Exception("GameObject is not active"));
				}
			}

			return promise;
		}

		public IPromise WaitFor(float seconds)
		{
			var promise = new Promise(this);

			if (!MonoBehaviour.Destroyed && MonoBehaviour.gameObject.activeInHierarchy)
			{
				var coroutine = MonoBehaviour.StartCoroutine(WaitHandler(seconds, () => { promise.SafeResolve(); }));
				promise.OnCanceled(() =>
				{
					if (!MonoBehaviour.Destroyed && MonoBehaviour.gameObject.activeInHierarchy)
						MonoBehaviour.StopCoroutine(coroutine);
				});
			}
			else
			{
				if (promise.IsPending)
				{
					promise.Reject(new Exception("CoroutinesService is not active"));
				}
			}

			return promise;
		}

		public IPromise WaitForRealtime(float seconds)
		{
			var promise = new Promise(this);

			if (!MonoBehaviour.Destroyed && MonoBehaviour.gameObject.activeInHierarchy)
			{
				var coroutine = MonoBehaviour.StartCoroutine(WaitRealtimeHandler(seconds, () => { promise.SafeResolve(); }));
				promise.OnCanceled(() =>
				{
					if (!MonoBehaviour.Destroyed && MonoBehaviour.gameObject.activeInHierarchy)
						MonoBehaviour.StopCoroutine(coroutine);
				});
			}
			else
			{
				if (promise.IsPending)
				{
					promise.Reject(new Exception("CoroutinesService is not active"));
				}
			}

			return promise;
		}

		public IPromise WaitFor(IEnumerator routine)
		{
			var promise = new Promise(this);

			if (!MonoBehaviour.Destroyed && MonoBehaviour.gameObject.activeInHierarchy)
			{
				IEnumerator Wait()
				{
					yield return routine;

					promise.SafeResolve();
				}

				MonoBehaviour.StartCoroutine(Wait());
			}
			else
			{
				if (promise.IsPending)
				{
					promise.Reject(new Exception("CoroutinesService is not active"));
				}
			}

			return promise;
		}

		public IPromise InvokeAfter(float seconds, Action callback)
		{
			var promise = new Promise(this);

			if (!MonoBehaviour.Destroyed && MonoBehaviour.gameObject.activeInHierarchy)
			{
				MonoBehaviour.StartCoroutine(WaitHandler(seconds, () =>
				{
					if (!promise.IsPending)
					{
						return;
					}

					callback.SafeInvoke();
					promise.Resolve();
				}));
			}
			else
			{
				promise.Reject(new Exception("CoroutineService is not active"));
			}

			return promise;
		}

		public IEnumerator GetExecutionLoopRoutine(IStateMachine machine)
		{
			return StateMachineTick(machine);
		}

		public IEnumerator ExecuteDelayRoutine(float delay, Action action)
		{
			yield return new WaitForSecondsRealtime(delay);
			action.SafeInvoke();
		}

		private static IEnumerator StateMachineTick(IStateMachine machine)
		{
			while (true)
			{
				machine?.Execute();
				yield return null;
			}
		}

		private static IEnumerator WaitHandler(float seconds, Action callback)
		{
			yield return new WaitForSeconds(seconds);

			callback.SafeInvoke();
		}
		
		private static IEnumerator WaitRealtimeHandler(float seconds, Action callback)
		{
			yield return new WaitForSecondsRealtime(seconds);

			callback.SafeInvoke();
		}

		private static IEnumerator WaitHandler(AsyncOperation asyncOperation, Action callback)
		{
			yield return asyncOperation;

			callback.SafeInvoke();
		}

		private static IEnumerator WaitFrameHandler(Promise promise)
		{
			yield return 0;

			promise.SafeResolve();
		}
		
		private static IEnumerator WaitFramesHandler(Promise promise, int framesCount)
		{
			for (var i = 0; i < framesCount; i++)
			{
				yield return 0;
			}

			promise.SafeResolve();
		}
	}
}