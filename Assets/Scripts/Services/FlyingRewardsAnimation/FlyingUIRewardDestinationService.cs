using Common.Currency;
using RSG;
using UnityEngine;

namespace Services.FlyingRewardsAnimation
{
    public class FlyingUIRewardDestinationService
    {
        public IPromise<Vector3> GetDestination(ICurrency currency)
        {
            switch (currency)
            {
                case Stars:
                    return Promise<Vector3>.Resolved(GetStarsDestination());
                default:
                    return Promise<Vector3>.Resolved(GetDefaultDestination());
            }
        }
        
        private Vector2 GetStarsDestination()
        {
            //TODO ADD LOGIC

            return GetDefaultDestination();
        }
        
        private Vector3 GetDefaultDestination()
        {
            return Vector3.zero;
        }
    }
}