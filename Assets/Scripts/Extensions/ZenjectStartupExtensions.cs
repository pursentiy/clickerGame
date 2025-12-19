using Installers;

namespace Extensions
{
    public static class ZenjectStartupExtensions
    {
        public static T Inject<T>(this T obj)
        {
            ContainerHolder.CurrentContainer.Inject(obj);
            return obj;
        }
    }
}