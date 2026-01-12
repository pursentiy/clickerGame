using UnityEngine;
using UnityEngine.UI;

namespace Common.Widgets
{
    [RequireComponent(typeof(Image))]
    public class ImageAlphaSetter : MonoBehaviour
    {
        [SerializeField] private AlphaIntensity _intensity = AlphaIntensity.Moderate;
    
        private Image _image;
    
        public void ApplyAlpha()
        {
            if (_image == null)
                _image = GetComponent<Image>();

            if (_image == null) return;

            // Получаем текущий цвет
            Color color = _image.color;
        
            // В Unity альфа задается от 0 до 1, поэтому делим значение enum на 255
            color.a = (float)_intensity / 255f;
        
            _image.color = color;
        }
    
        public void SetIntensity(AlphaIntensity newIntensity)
        {
            _intensity = newIntensity;
            ApplyAlpha();
        }

        private void OnValidate()
        {
            ApplyAlpha();
        }

        private void Awake()
        {
            ApplyAlpha();
        }
    }

    public enum AlphaIntensity
    {
        Light = 75,      // ~30% прозрачности
        Moderate = 150,  // ~60% прозрачности
        Heavy = 240      // ~95% прозрачности
    }
}