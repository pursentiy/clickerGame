using UnityEngine;

namespace Common.Utilities
{
    public class DecimalRange
    {
        [System.Serializable]
        public class FloatRange
        {
            [SerializeField] private float _min;
            [SerializeField] private float _max;

            public float Min => _max > _min ? _min : _max;
            public float Max => _max > _min ? _max : _min;

            public FloatRange(float min, float max)
            {
                _min = min;
                _max = max;
            }

            public float GetRandomValue()
            {
                return Random.Range(Min, Max);
            }

            public bool IsEmpty()
            {
                return Mathf.Approximately(_min, 0f) && Mathf.Approximately(_max, 0f);
            }
        }
    }
}