using Common.Currency;
using Storage.Snapshots.LevelParams;

namespace Services
{
    public class LevelHelperService
    {
        public bool IsLevelCompeted(LevelParamsSnapshot levelParams)
        {
            if (levelParams == null)
            {
                return false;
            }

            var progress = levelParams.LevelFiguresParamsList.TrueForAll(levelFigureParams =>
                levelFigureParams.Completed);

            return progress;
        }

        public int EvaluateStarsForAccrual(Stars newEarnedStars, int? maybeOldEarnedStars)
        {
            if (newEarnedStars <= 0)
                return 0;
            
            if (!maybeOldEarnedStars.HasValue)
                return newEarnedStars;

            if (maybeOldEarnedStars.Value >= newEarnedStars)
                return 0;
            
            return newEarnedStars - maybeOldEarnedStars.Value;
        }

        public int EvaluateEarnedStars(LevelParamsSnapshot levelParams, float levelCompletionTime)
        {
            if (levelCompletionTime <= 0)
                return 0;

            var beatingTimeInfo = levelParams.LevelBeatingTimeInfo;
            if (beatingTimeInfo.FastestTime >= levelCompletionTime)
            {
                return 3;
            }
            
            if (beatingTimeInfo.MediumTime >= levelCompletionTime)
            {
                return 2;
            }
            
            if (beatingTimeInfo.MinimumTime >= levelCompletionTime)
            {
                return 1;
            }

            return 0;
        }
    }
}