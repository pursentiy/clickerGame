using UnityEngine;

namespace Utilities.Disposable
{
	public class ComponentDisposeProvider : MonoBehaviour, IDisposeProvider
	{
		public DisposableCollection ChildDisposables => _collection ?? (_collection = new DisposableCollection());
		
		private DisposableCollection _collection;

		private void OnDestroy()
		{
			DisposeService.HandledDispose(this);
		}
	}
}