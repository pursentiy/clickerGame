using System;
using Common.Handlers.Draggable;
using Services;
using UI.Screens.PuzzleAssembly.Figures;

namespace Extensions
{
    public static class DraggableExtensions
    {
        public static T GetAs<T>(this IDraggable draggable) where T : class
        {
            if (draggable is T result)
            {
                return result;
            }
            
            LoggerService.LogWarning($"[{nameof(DraggableExtensions)}] {nameof(GetAs)}.<{typeof(T)}> returned null.");
            return null;
        }
    }
}