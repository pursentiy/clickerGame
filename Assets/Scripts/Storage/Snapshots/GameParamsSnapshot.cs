namespace Storage.Snapshots
{
    public class GameParamsSnapshot
    {
        public GameParamsSnapshot(bool isMusicOn, bool isSoundOn, string language)
        {
            IsMusicOn = isMusicOn;
            IsSoundOn = isSoundOn;
            Language = language;
        }

        public bool IsMusicOn { get; set; }
        public bool IsSoundOn { get; set; }
        public string Language { get; set; }
    }
}