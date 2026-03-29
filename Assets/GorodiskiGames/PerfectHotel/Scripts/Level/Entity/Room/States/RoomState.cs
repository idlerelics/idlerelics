using Game.Core;
using Injection;

namespace Game.Level.Room
{
    public abstract class RoomState : State
    {
        [Inject] protected RoomController _room;
        [Inject] protected Timer _timer;
    }
}


