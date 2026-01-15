using GameState.OnGameEnterSequence;
using UnityEngine;
using Utilities.Disposable;
using Utilities.StateMachine;

namespace GameState
{
    public class MainSceneLoader : MonoBehaviour
    {
        private StateMachine _machine;
        
        private void Start()
        {
            _machine = new StateMachine(null);
            _machine.StartSequence<StartServicesState>().FinishWith(this);
        }
    }
}