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
        
        private int _lastScreenWidth;
        private int _lastScreenHeight;
        private float _lastOrthoSize;
        private Vector3 _lastCameraPos;
        private float _lastTargetWidth;
        private float _lastTargetHeight;
        private float _lastPpu;
        private bool _lastAlign;
        
        public void ApplyUniversalFill(bool force = false)
        {
            if (Camera.main == null) return;

            // Проверяем, изменилось ли что-то в окружении или настройках
            if (!force && !HasChanges()) return;

            // 1. Get Camera Dimensions
            var cam = Camera.main;
            var worldScreenHeight = cam.orthographicSize * 2.0f;
            var screenWidth = UnityEngine.Device.Screen.width;
            var screenHeight = UnityEngine.Device.Screen.height;
            var aspectRatio = (float)screenWidth / screenHeight;
            var worldScreenWidth = worldScreenHeight * aspectRatio;

            // 2. Calculate "Natural" World Size
            var spriteWorldWidth = targetWidth / ppu;
            var spriteWorldHeight = targetHeight / ppu;

            // 3. Calculate Scale Factors
            float scaleX = worldScreenWidth / spriteWorldWidth;
            float scaleY = worldScreenHeight / spriteWorldHeight;

            // 4. Final Scale
            float finalScale = Mathf.Max(scaleX, scaleY);
            transform.localScale = new Vector3(finalScale, finalScale, 1f);

            // 5. Align to Bottom
            if (alignToBottom)
            {
                float scaledSpriteHeight = spriteWorldHeight * finalScale;
                float cameraBottomY = cam.transform.position.y - (worldScreenHeight / 2f);
                float newY = cameraBottomY + (scaledSpriteHeight / 2f);
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }

            // Обновляем кэш после успешного применения
            UpdateCache(screenWidth, screenHeight, cam);
        }

        private bool HasChanges()
        {
            var cam = Camera.main;
            return UnityEngine.Device.Screen.width != _lastScreenWidth ||
                   UnityEngine.Device.Screen.height != _lastScreenHeight ||
                   !Mathf.Approximately(cam.orthographicSize, _lastOrthoSize) ||
                   cam.transform.position != _lastCameraPos ||
                   !Mathf.Approximately(targetWidth, _lastTargetWidth) ||
                   !Mathf.Approximately(targetHeight, _lastTargetHeight) ||
                   !Mathf.Approximately(ppu, _lastPpu) ||
                   alignToBottom != _lastAlign;
        }

        private void UpdateCache(int w, int h, Camera cam)
        {
            _lastScreenWidth = w;
            _lastScreenHeight = h;
            _lastOrthoSize = cam.orthographicSize;
            _lastCameraPos = cam.transform.position;
            _lastTargetWidth = targetWidth;
            _lastTargetHeight = targetHeight;
            _lastPpu = ppu;
            _lastAlign = alignToBottom;
        }
    }
}