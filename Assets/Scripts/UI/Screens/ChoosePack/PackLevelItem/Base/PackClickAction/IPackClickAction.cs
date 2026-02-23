using UnityEngine;

namespace UI.Screens.ChoosePack.PackLevelItem.Base.PackClickAction
{
    /// <summary>
    /// Context passed to the pack click action. Implement or use <see cref="PackClickAction"/> to pass whatever data the handler needs (e.g. popup anchor).
    /// </summary>
    public interface IPackClickAction
    {
        RectTransform PopupAnchorRect { get; }
    }

    /// <summary>
    /// Default implementation of <see cref="IPackClickAction"/>. Can be extended with more properties as needed.
    /// </summary>
    public class PackClickAction : IPackClickAction
    {
        public PackClickAction(RectTransform popupAnchorRect)
        {
            PopupAnchorRect = popupAnchorRect;
        }

        public RectTransform PopupAnchorRect { get; }
    }
}
