using UnityEngine;

namespace Handlers.UISystem
{
    public class UISystemData : ScriptableObject
    {
        [SerializeField] public Canvas _uiCanvasPrefab;
        [SerializeField] public GameObject _uiScreenParticlesContainerPrefab;
        
        public GameObject UICanvasPrefab => _uiCanvasPrefab.gameObject;
        public GameObject UiScreenParticlesContainerPrefab => _uiScreenParticlesContainerPrefab;
    }
}