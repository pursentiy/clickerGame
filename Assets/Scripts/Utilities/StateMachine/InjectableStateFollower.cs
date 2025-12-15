using Installers;
using Platform.Common.Utilities.StateMachine;

namespace Utilities.StateMachine
{
    public abstract class InjectableStateFollower : IStateFollower
    {
        public virtual void OnEnter()
        {
            ContainerHolder.CurrentContainer.Inject(this);
        }

        public virtual void OnExit()
        {
            
        }
    }
}