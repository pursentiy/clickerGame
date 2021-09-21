using System.Collections.Generic;
using Figures;
using Figures.Animals;
using Plugins.FSignal;
using Storage.Levels.Params;
using UnityEngine.EventSystems;

namespace Level.Hud
{
    public interface ILevelHudHandler
    {
        void SetupScrollMenu(List<LevelFigureParams> levelFiguresParams);
        void LockScroll(bool value);
        FigureMenu GetFigureById(int figureId);
        List<FSignal<FigureMenu>> GetOnBeginDragFiguresSignal();
        List<FSignal<PointerEventData>> GetOnDragEndFiguresSignal();
    }
}