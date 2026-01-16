using UnityEngine;

namespace Common.Widgets
{
    public class CommonLoadingSpinner : MonoBehaviour
    {
        [SerializeField] private Transform _spinnerTransform;
        [SerializeField] private float _rotateSpeed = -630;

        void Update()
        {
            if (_spinnerTransform != null)
            {
                _spinnerTransform.Rotate(0,0, _rotateSpeed * Time.deltaTime);
            }
        }
    }
}