using Level.Hud;
using UnityEngine;

namespace Handlers
{
    public class LevelParamsHandler : MonoBehaviour, ILevelParamsHandler
    {
        [SerializeField] private LevelHudHandler _levelHudHandlerPrefab;
        [SerializeField] private Color _targetFigureDefaultColor;
        
        public LevelHudHandler LevelHudHandlerPrefab => _levelHudHandlerPrefab;
        public Color TargetFigureDefaultColor => _targetFigureDefaultColor;
    }
}