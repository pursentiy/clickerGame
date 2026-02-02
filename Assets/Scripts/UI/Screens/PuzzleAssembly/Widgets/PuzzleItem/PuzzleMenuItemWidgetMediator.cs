using ThirdParty.SuperScrollView.Scripts.List;

namespace UI.Screens.PuzzleAssembly.Widgets.PuzzleItem
{
    public class PuzzleMenuItemWidgetMediator : InjectableListItemMediator<PuzzleMenuItemWidgetView, PuzzleMenuItemWidgetInfo>
    {
        public PuzzleMenuItemWidgetMediator(PuzzleMenuItemWidgetInfo data) : base(data)
        {
        }

        protected override void OnInitialize(bool isVisibleOnRefresh)
        {
            base.OnInitialize(isVisibleOnRefresh);

        }
    }
}