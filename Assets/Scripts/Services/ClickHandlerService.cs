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
            var results = new List<RaycastResult>();
            raycaster.Raycast(eventData, results);
            
            var figures = new List<FigureTargetWidget>();
            foreach (var result in results)
            {
                var figure = result.gameObject.GetComponent<FigureTargetWidget>();

                if (figure == null) 
                    continue;
                 
                figures.Add(figure);
            }

            return figures;
        }
    }
}