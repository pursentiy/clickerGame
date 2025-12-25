namespace Storage.Snapshots.LevelParams
{
    public class LevelFigureParamsSnapshot
    {
        public int FigureId { get; }
        public bool Completed { get; private set; }
        
        public LevelFigureParamsSnapshot(int figureId, bool completed)
        {
            FigureId = figureId;
            Completed = completed;
        }

        public void SetLevelCompleted(bool completed)
        {
            Completed = completed;
        }
    }
}