using Zenject;

namespace Installers
{
    public static class ContainerHolder
    {
        public static DiContainer CurrentContainer { get; private set; }

        public static void SetCurrentContainer(DiContainer container)
        {
            CurrentContainer = container;
        }
    }
}