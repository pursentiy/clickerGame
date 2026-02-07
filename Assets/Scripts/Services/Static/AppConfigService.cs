
namespace Services.Static
{
    public static class AppConfigService
    {
        private static readonly Environment _currentEnv = Environment.Production;
        
        public static string Version = "1.0.1";
        
        public static bool IsProduction() => _currentEnv == Environment.Production;
        public static bool IsDebug() => _currentEnv == Environment.Production;
    }
    
    public enum Environment
    {
        Debug, // Локальные файлы (ПК / Редактор)
        Production // Playgama Bridge (Яндекс, FB, ВК)
    }
}