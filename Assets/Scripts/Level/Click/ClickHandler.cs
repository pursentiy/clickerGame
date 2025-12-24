using Components.Levels.Figures;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Level.Click
{
    public class ClickHandler : MonoBehaviour, IClickHandler
    {
        private Camera _camera;

        private void Awake()
        {
            _camera = Camera.main;
        }

        public FigureTarget TryGetFigureAnimalTargetOnDragEnd(PointerEventData eventData)
        {
            var resultsData =  Physics2D.Raycast(_camera.ScreenToWorldPoint(eventData.position), Vector2.zero);
            
            if (resultsData.collider == null)
            {
                return null;
            }
            
            var raycastResult = resultsData.transform.gameObject.GetComponent<FigureTarget>();
            return raycastResult.gameObject == null ? null : raycastResult.gameObject.GetComponent<FigureTarget>();
        }
    }
}