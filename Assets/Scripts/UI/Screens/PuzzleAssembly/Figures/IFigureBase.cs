namespace UI.Screens.PuzzleAssembly.Figures
{
    public interface IFigureBase
    {
        public bool IsCompleted { get; }
        public int Id { get; }
        public void SetFigureCompleted(bool value);
    }
}