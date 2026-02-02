using Common.Handlers.Draggable;
using ThirdParty.SuperScrollView.Scripts.List;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens.PuzzleAssembly.Widgets.PuzzleItem
{
    public class PuzzleMenuItemWidgetView : ItemViewBase
    {
        public Image _image;
        public RectTransform _transformFigure;
        public RectTransform _transformContainer;
        public ParticleSystem _particleSystem;
        public DraggableItemHandler DraggableItemHandler;
    }
}