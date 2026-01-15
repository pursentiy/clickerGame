using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Services
{
    public class ScenesManagerService
    {
        private const string _bootstrapperSceneName = "BootstrapperSceneName";
        private const string _mainSceneName = "MainSceneName";
        
        private readonly Dictionary<SceneTypes, string> _scenes;

        public ScenesManagerService()
        {
            _scenes = new Dictionary<SceneTypes, string>
            {
                { SceneTypes.BootstrapperScene, _bootstrapperSceneName },
                { SceneTypes.MainScene, _mainSceneName }
            };
        }
    
        public void LoadScene(SceneTypes sceneType)
        {
            if (_scenes.TryGetValue(sceneType, out string sceneName))
            {
                LoggerService.LogDebug($"[{GetType().Name}] Loading scene: {sceneName}");
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                LoggerService.LogError($"[{GetType().Name}] Scene name for {sceneType} is not defined in the dictionary!");
            }
        }
        
    }

    public enum SceneTypes
    {
        BootstrapperScene,
        MainScene,
    }
}