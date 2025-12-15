using UnityEngine;

namespace Common
{
    public class EmptyMonoBehaviour : MonoBehaviour
    {
        public bool Destroyed { get; private set; }

        private void OnDestroy()
        {
            Destroyed = true;
        }
    }
}