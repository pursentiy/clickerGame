using System.Collections.Generic;
using Figures;
using Figures.Animals;
using Plugins.FSignal;
using RSG;
using Storage.Levels.Params;
using UnityEngine.EventSystems;

namespace Level.Hud
{
    public interface ILevelHudHandler
    {
        void SetInteractivity(bool isInteractable);
        void SetupScrollMenu(List<LevelFigureParams> levelFiguresParams);
        void LockScroll(bool value);
        FigureMenu GetFigureById(int figureId);
        void DestroyFigure(int figureId);
        void ShiftAllElements(bool isInserting, int figureId, Promise animationPromise);
        void TryShiftAllElementsAfterRemoving(int figureId, Promise animationPromise);
        List<FSignal<FigureMenu>> GetOnBeginDragFiguresSignal();
        List<FSignal<PointerEventData>> GetOnDragEndFiguresSignal();
    }
}