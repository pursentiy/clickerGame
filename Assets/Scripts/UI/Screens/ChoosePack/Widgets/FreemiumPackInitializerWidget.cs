using Common.Currency;
using Common.Data.Info;
using Configurations.Progress;
using ThirdParty.SuperScrollView.Scripts.List;
using UI.Screens.ChoosePack.PackLevelItem.Base;
using UI.Screens.ChoosePack.PackLevelItem.FreemiumPackItem;

namespace UI.Screens.ChoosePack.Widgets
{
    public class FreemiumPackInitializerWidget : BasePackInitializerWidget
    {
        protected override PackType TargetPackType => PackType.Freemium;

        protected override IListItem CreateMediator(BasePackItemWidgetInfo info)
        {
            return new FreemiumPackItemWidgetMediator((FreemiumPackItemWidgetInfo)info);
        }

        protected override BasePackItemWidgetInfo CreatePackWidgetInfoInternal(PackInfo packInfo, int packId, bool isUnlocked, Stars starsRequired)
        {
            return new FreemiumPackItemWidgetInfo(
                packInfo.PackName, 
                packInfo.PackImagePrefab, 
                packId, 
                isUnlocked,
                () => OnAvailablePackClicked(packInfo), 
                OnUnavailablePackClicked, 
                starsRequired);
        }
    }
}
