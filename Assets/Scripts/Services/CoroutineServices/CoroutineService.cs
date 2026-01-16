using Common;
using JetBrains.Annotations;
using UnityEngine;

namespace Services.CoroutineServices
{
    [UsedImplicitly]
    public class CoroutineService : CoroutineServiceBase, ICoroutineService
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
}