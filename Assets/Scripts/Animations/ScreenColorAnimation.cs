using UnityEngine;
using UnityEngine.UI;

namespace Animations
{
    public class ScreenColorAnimation : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _backgroundTexture;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Gradient _gradient;
        [SerializeField] private float _colorChangingDuration;
        [SerializeField] private bool _canBeAnimated;

        private float _time;
        private bool _isIncreasing = true;

        public Gradient Gradient => _gradient;

        public void StartColorLoop(Gradient gradient)
        {
            _gradient = gradient;
            _canBeAnimated = true;
        }

        private void Update()
        {
            if(!_canBeAnimated)
                return;
            
            AnimateBackgroundColor();
        }

        private void AnimateBackgroundColor()
        {
            var value = Mathf.Lerp(0f, 1f, _time);

            if (_isIncreasing)
            {
                _time += Time.deltaTime / _colorChangingDuration;

                if (_time >= 1)
                {
                    _isIncreasing = false;
                }
                
            }
            else
            {
                _time -= Time.deltaTime / _colorChangingDuration;
                
                if (_time <= 0)
                {
                    _isIncreasing = true;
                }
            }
            

            var color = _gradient.Evaluate(value);
            
            if(_backgroundImage)
                _backgroundImage.color = color;
            
            if(_backgroundTexture)
                _backgroundTexture.color = color;
        }
    }
}
