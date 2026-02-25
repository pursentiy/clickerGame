using UnityEngine;

namespace UI.Popups.DailyRewardPopup.DailyRewardDayItem
{
    public class DailyRewardAlreadyReadyToCollectView : DailyRewardViewBase
    {
        [SerializeField] private ParticleSystem glowParticles;
        [SerializeField] private ParticleSystem dustParticles;

        public void PlayGlow()
        {
            if (glowParticles != null && !glowParticles.isPlaying)
                glowParticles.Play();
        }

        public void StopGlow()
        {
            if (glowParticles != null)
                glowParticles.Stop();
        }

        public void PlayDust()
        {
            if (dustParticles != null)
                dustParticles.Play();
        }
    }
}
