using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Animations
{
    public class CloudAnimation : MonoBehaviour
    {
        [SerializeField] private float _startX;
        [SerializeField] private float _endX;
        [SerializeField] private float _duration;
        [SerializeField] private float _delay;
        [SerializeField] private float _minY;
        [SerializeField] private float _maxY;
        [SerializeField] private RectTransform _transform;
   
        private Coroutine _cloudCoroutine;

        private void Start()
        {
            _cloudCoroutine = StartCoroutine(Animation());
        }

        private IEnumerator Animation()
        {
            _transform.localPosition = new Vector2(_startX, GetRandomValue());

            var t = Random.Range(0f, 1f);
        
            while (true)
            {
                var y = GetRandomValue();
                var startPosition = new Vector2(_startX, y);
                var endPosition = new Vector2(_endX, y);

                while (t < 1)
                {
                    _transform.localPosition = Vector2.Lerp(startPosition, endPosition, t);
                    yield return null;
                    t += Time.deltaTime / _duration;
                }

                t = 0;
            
                yield return new WaitForSeconds(_delay);
            }
        }

        private float GetRandomValue()
        {
            return Random.Range(_minY, _maxY);
        }

        private void OnDestroy()
        {
            if(_cloudCoroutine != null)
                StopCoroutine(_cloudCoroutine);
        }
    }
}
