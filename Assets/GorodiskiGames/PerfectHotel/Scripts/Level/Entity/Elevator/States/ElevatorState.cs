using Game.Core;
using Injection;

namespace Game.Level.Elevator
{
    public abstract class ElevatorState : State
    {
        [Inject] protected ElevatorController _elevator;
        [Inject] protected Timer _timer;
    }
}

