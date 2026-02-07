namespace Extensions
{
    public static class AudioExtensions
    {
        public static float EarnedStarPitchCoefficient = 1.15f;
        
        public static string StartLevelKey => "start_level";
        public static string LevelFinishedKey => "level_finished";
        public static string PackUnlockedKey => "pack_unlocked";
        public static string FailPuzzleInsertionKey => "puzzle_insertion_fail";
        public static string SuccessPuzzleInsertionKey => "puzzle_insertion_success";
        public static string StarEarnedKey => "star_earned";
        public static string LevelFailedKey => "level_failed";
        public static string PopupAppearKey => "popup_appear";
        public static string PopupHideKey => "popup_hide";
        public static string ResourcesCollected => "resources_collected";
        public static string BumpPanelKey => "bump_panel";
    }
}