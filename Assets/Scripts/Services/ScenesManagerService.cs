using UnityEngine.SceneManagement;

namespace Services
{
    public class ScenesManagerService
    {
        public void LoadScene(SceneTypes sceneType)
        {
            LoggerService.LogDebug($"[{GetType().Name}] Loading scene: {sceneType}");
            SceneManager.LoadScene((int)sceneType);
        }
    }

    public enum SceneTypes
    {
        MainScene = 1,
    }
}