using Plugins.FSignal;

namespace Services
{
    public class PlayerService
    {
        public int Stars {get; private set;}
        public FSignal<int> StarsChangedSignal = new FSignal<int>();

        public void Initialize(int stars)
        {
            Stars = stars;
        }

        public void AddStars(int amount)
        {
            Stars += amount;
            StarsChangedSignal.Dispatch(Stars);
        }
    }
}