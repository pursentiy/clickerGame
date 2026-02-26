using UnityEngine;
using Utilities;

namespace UI.Popups.DailyRewardPopup.DailyRewardDayItem.Animations
{
    public interface IDailyRewardAnimationContext
    {
        RectTransform ContentHolder { get; }
        Canvas ItemCanvas { get; }
        CanvasGroup ItemCanvasGroup { get; }
        IDisposeProvider DisposeProvider { get; }
    
        float ReadyBounce { get; }
        int InitialSortingOrder { get; }
        Vector3 InitialScale { get; }
        Vector2 InitialPos { get; }

        void PlayGlow();
        void PlayDust();
    }
}