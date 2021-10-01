using UnityEngine;

namespace GlobalParams
{
    public class GlobalParams : MonoBehaviour
    {
        private void Awake()
        {
            Input.multiTouchEnabled = false;
        }
    }
}