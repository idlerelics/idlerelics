using Game.Core;
using Injection;

namespace Game.Level.Utility.UtilityStates
{
    public abstract class UtilityState : State
    {
        [Inject] protected UtilityController _utility;
        [Inject] protected GameManager _gameManager;

        public override void Dispose()
        {
        }

        public override void Initialize()
        {
        }
    }
}


