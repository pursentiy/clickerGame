using Common.Currency;
using Common.Data.Info;
using Configurations.Progress;
using ThirdParty.SuperScrollView.Scripts.List;
using UI.Screens.ChoosePack.PackLevelItem.Base;
using UI.Screens.ChoosePack.PackLevelItem.DefaultPackItem;

namespace UI.Screens.ChoosePack.Widgets
{
    public class DefaultPackInitializerWidget : BasePackInitializerWidget
    {
        protected override PackType TargetPackType => PackType.Default;

        protected override IListItem CreateMediator(BasePackItemWidgetInfo info)
        {
            return new DefaultPackItemWidgetMediator((DefaultPackItemWidgetInfo)info);
        }

        protected override BasePackItemWidgetInfo CreatePackWidgetInfoInternal(PackInfo packInfo, int packId, bool isUnlocked, ICurrency currencyToUnlock, int indexInList, System.Func<bool> getEntranceAnimationsAlreadyTriggered)
        {
            return new DefaultPackItemWidgetInfo(
                packInfo.PackName,
                packInfo.PackImagePrefab,
                packId,
                isUnlocked,
                () => OnAvailablePackClicked(packInfo),
                OnUnavailablePackClicked,
                currencyToUnlock,
                indexInList,
                getEntranceAnimationsAlreadyTriggered);
        }
    }
}
