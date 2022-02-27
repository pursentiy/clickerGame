namespace Handlers
{
    public interface ISoundHandler
    {
       void PlaySound(string clipName);
       void PlayButtonSound();
       void StartAmbience(string exceptClipName = "");
       void SetSoundVolume(bool isOn);
       void SetMusicVolume(bool isOn);
    }
}