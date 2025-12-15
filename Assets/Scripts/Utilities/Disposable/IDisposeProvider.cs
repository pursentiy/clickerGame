using Utilities.Disposable;

namespace Utilities
{
	public interface IDisposeProvider
	{
		DisposableCollection ChildDisposables { get; }
	}
}