using System.Diagnostics;
using Installers;
using Platform.Common.Utilities.StateMachine;

namespace Utilities.StateMachine
{
    public abstract class InjectableStateBaseWithTimer<TContext> : StateBase<TContext> where TContext : class, IStateContext
    {
        protected readonly Stopwatch Watch = new Stopwatch();

        protected static bool CutOffTimeLogging = false;

        public override void OnEnter(params object[] arguments)
        {
            Watch.Start();
            
            ContainerHolder.CurrentContainer.Inject(this);
            
            base.OnEnter(arguments);
        }

        public override void OnExit()
        {
            base.OnExit();
            Watch.Stop();
        }

        protected virtual string GetNameForLog()
        {
            return GetType().Name;
        }
    }
}