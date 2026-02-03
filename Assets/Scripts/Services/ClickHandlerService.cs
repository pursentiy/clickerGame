using System.Collections.Generic;
using UI.Screens.PuzzleAssembly.Figures;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Services
{
    public class ClickHandlerService
    {
        public List<FigureTargetWidget> DetectFigureTarget(PointerEventData eventData, GraphicRaycaster raycaster)
        {
            var raycastResults = UnityEngine.Pool.ListPool<RaycastResult>.Get();
            var foundFigures = new List<FigureTargetWidget>();

            try
            {
                raycaster.Raycast(eventData, raycastResults);

                foreach (var result in raycastResults)
                {
                    if (result.gameObject.TryGetComponent<FigureTargetWidget>(out var figure))
                    {
                        foundFigures.Add(figure);
                    }
                }
            }
            finally
            {
                UnityEngine.Pool.ListPool<RaycastResult>.Release(raycastResults);
            }

            return foundFigures;
        }
    }
}