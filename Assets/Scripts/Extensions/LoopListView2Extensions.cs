using ThirdParty.SuperScrollView.Scripts.GridView;
using ThirdParty.SuperScrollView.Scripts.ListView;

namespace Extensions
{
    public static class LoopListView2Extensions
    {
        public static LoopListViewInitParam GetInitListParams()
        {
            var initParams = LoopListViewInitParam.CopyDefaultInitParam();
            initParams.mDistanceForNew0 *= 4;
            initParams.mDistanceForNew1 *= 4;
            initParams.mDistanceForRecycle0 *= 4;
            initParams.mDistanceForRecycle1 *= 4;

            return initParams;
        }
        
        public static LoopGridViewInitParam GetInitGridParams()
        {
            var initParams = LoopGridViewInitParam.CopyDefaultInitParam();

            return initParams;
        }
    }
}