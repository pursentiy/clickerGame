using UnityEngine;

namespace Handlers.UISystem
{
    public class UISystemData : ScriptableObject
    {
        [SerializeField] public Canvas _uiCanvasPrefab;
        
        public GameObject UICanvasPrefab => _uiCanvasPrefab.gameObject;
    }
}