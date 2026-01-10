using System.Collections.Generic;
using Components.Levels.Figures;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Services
{
    public class ClickHandlerService
    {
        public FigureTarget DetectFigureTarget(PointerEventData eventData, GraphicRaycaster raycaster)
        {
            var results = new List<RaycastResult>();
            raycaster.Raycast(eventData, results);
            
            foreach (var result in results)
            {
                var figure = result.gameObject.GetComponent<FigureTarget>();

                if (figure == null) 
                    continue;
                return figure;
            }

            return null;
        }
    }
}