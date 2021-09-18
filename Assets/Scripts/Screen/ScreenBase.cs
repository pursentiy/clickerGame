using Installers;

namespace Screen
{
    public abstract class ScreenBase : InjectableMonoBehaviour
    {
        protected int _currentLevel = -1;
        
        public int CurrentLevel
        {
            set => _currentLevel = value;
        }
    }
}