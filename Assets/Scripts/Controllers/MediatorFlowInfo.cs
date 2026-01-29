using RSG;

namespace Controllers
{
    public class MediatorFlowInfo
    {
        public MediatorFlowInfo(IPromise mediatorLoadPromise, IPromise mediatorHiddenPromise)
        {
            MediatorLoadPromise = mediatorLoadPromise;
            MediatorHiddenPromise = mediatorHiddenPromise;
        }

        public IPromise MediatorLoadPromise { get; }
        public IPromise MediatorHiddenPromise { get; }
    }
}