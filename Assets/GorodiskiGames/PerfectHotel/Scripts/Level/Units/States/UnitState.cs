using Game.Core;
using Injection;

namespace Game.Level.Unit
{
    public abstract class UnitState : State
    {
        [Inject] protected UnitController _unit;
        [Inject] protected Timer _timer;
    }
}