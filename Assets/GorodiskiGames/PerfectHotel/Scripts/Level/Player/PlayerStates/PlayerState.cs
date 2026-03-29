using Game.Core;
using Injection;

namespace Game.Level.Player
{
    public abstract class PlayerState : State
    {
        [Inject] protected PlayerController _player;
    }
}