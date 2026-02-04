using RSG;

namespace Controllers
{
    public class MediatorFlowInfo
    {
        public MediatorFlowInfo(IPromise mediatorLoadPromise, IPromise mediatorHidePromise)
        {
            MediatorLoadPromise = mediatorLoadPromise;
            MediatorHidePromise = mediatorHidePromise;
        }

        public IPromise MediatorLoadPromise { get; }
        public IPromise MediatorHidePromise { get; }
    }
}