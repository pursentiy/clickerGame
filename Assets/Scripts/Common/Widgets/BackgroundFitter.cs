using UnityEngine;

namespace Common.Widgets
{
    public class BackgroundFitter : MonoBehaviour
    {
        [Header("Reference Resolution")]
        [Tooltip("The width of the sprite in pixels (e.g., 1920)")]
        public float targetWidth = 1920f;
        [Tooltip("The height of the sprite in pixels (e.g., 1080)")]
        public float targetHeight = 1080f;
        [Tooltip("Pixels Per Unit from the sprite's import settings (default is 100)")]
        public float ppu = 100f;

        [Header("Alignment")]
        [Tooltip("If true, the bottom of the sprite will always touch the bottom of the screen")]
        public bool alignToBottom = true;

        void Start()
        {
            ApplyUniversalFill();
        }

#if UNITY_EDITOR
        void Update() => ApplyUniversalFill();
#endif
        
        public void ApplyUniversalFill()
        {
            // 1. Get Camera Dimensions in World Units
            var worldScreenHeight = Camera.main.orthographicSize * 2.0f;
            var aspectRatio = (float)UnityEngine.Device.Screen.width / UnityEngine.Device.Screen.height; // Cast to float is important!
            var worldScreenWidth = worldScreenHeight * aspectRatio;

            // 2. Calculate "Natural" World Size of the Sprite
            var spriteWorldWidth = targetWidth / ppu;
            var spriteWorldHeight = targetHeight / ppu;

            // 3. Calculate Scale Factors for both axes
            float scaleX = worldScreenWidth / spriteWorldWidth;
            float scaleY = worldScreenHeight / spriteWorldHeight;

            // 4. Use the LARGER scale factor to ensure "Fill" (no gaps)
            // If scaleX is 1.2 and scaleY is 1.5, we use 1.5. 
            // The width will be "over-scaled" but the height will fit perfectly.
            float finalScale = Mathf.Max(scaleX, scaleY);
            transform.localScale = new Vector3(finalScale, finalScale, 1f);

            // 5. Align to Bottom
            if (alignToBottom)
            {
                // Calculate the actual height of the sprite AFTER scaling
                float scaledSpriteHeight = spriteWorldHeight * finalScale;
            
                float cameraBottomY = Camera.main.transform.position.y - (worldScreenHeight / 2f);
            
                // Shift the center up by half the scaled height from the bottom edge
                float newY = cameraBottomY + (scaledSpriteHeight / 2f);
            
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }
        }
    }
}