using GameState.OnGameEnterSequence;
using Handlers.UISystem;
using UnityEngine;
using Zenject;

namespace Services
{
    public class GameStartupService : IInitializable
    {
        private readonly UIManager _uiManager;
        

        public void Initialize()
        {
            // ЭТА ТОЧКА — идеальный момент. 
            // Все инжекты в игре выполнены. 
            // Все MonoBehaviour на сцене получили свои зависимости.
            Debug.Log("Все системы Zenject инициализированы. Запускаем стейт-машину.");

        }
    }
}