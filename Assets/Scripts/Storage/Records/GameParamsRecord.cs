namespace Storage.Records
{
    public class GameParamsRecord
    {
        public bool IsMusicOn;
        public bool IsSoundOn;
        public string Language;

        public GameParamsRecord(bool isMusicOn, bool isSoundOn, string language)
        {
            IsMusicOn = isMusicOn;
            IsSoundOn = isSoundOn;
            Language = language;
        }
    }
}