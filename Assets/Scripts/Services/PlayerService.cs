using Plugins.FSignal;
using Storage.Snapshots;
using UnityEngine;

namespace Services
{
    public class PlayerService
    {
        public ProfileSnapshot ProfileSnapshot { get; private set; }
        public FSignal<int> StarsChangedSignal = new FSignal<int>();

        public void Initialize(ProfileSnapshot profileSnapshot)
        {
            ProfileSnapshot = profileSnapshot;
        }

        public void AddStars(int amount)
        {
            if (ProfileSnapshot == null)
            {
                Debug.LogError("ProfileSnapshot is null");
                return;
            }
            
            ProfileSnapshot.Stars += amount;
            StarsChangedSignal.Dispatch(ProfileSnapshot.Stars);
        }
    }
}