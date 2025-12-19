using System;
using Handlers.UISystem;
using Handlers.UISystem.Popups;

namespace Extensions
{
    public static class UIExtensions
    {
        public static TPopup GetFirst<TPopup>(this UIPopupsHandler handler) where TPopup : UIPopupBase
        {
            return handler.GetFirst(typeof(TPopup)) as TPopup;
        }

        public static UIPopupBase GetFirst(this UIPopupsHandler handler, Type type)
        {
            var popups = handler.GetShownPopups(type);
            if (popups?.Count > 0)
                return popups[0];
            return null;
        }
    }
}