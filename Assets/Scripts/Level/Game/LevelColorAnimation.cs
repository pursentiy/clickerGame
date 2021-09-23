using UnityEngine;

namespace Level.Game
{
    public class LevelColorAnimation : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _backgroundTexture;
        
        [SerializeField] private Gradient _gradient;
        [SerializeField] private float colorChangingDuration;
        private float _time;

        private void Update() {
            var value = Mathf.Lerp(0f, 1f, _time);
            _time += Time.deltaTime / colorChangingDuration;
            
            var color = _gradient.Evaluate(value);
            _backgroundTexture.color = color;
        }
    }
}
