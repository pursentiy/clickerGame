using Common;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Services.CoroutineServices
{
	[UsedImplicitly]
    public class PersistentCoroutinesService : CoroutineServiceBase, ICoroutineService
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
	

}