using System;
using Extensions;

namespace Utilities.Disposable
{
	public class DeferredDisposable : IDisposable
	{
		private readonly Action _disposeAction;

		public DeferredDisposable(Action disposeAction)
		{
			_disposeAction = disposeAction;
		}

		public void Dispose()
		{
			_disposeAction.SafeInvoke();
		}
	}
}