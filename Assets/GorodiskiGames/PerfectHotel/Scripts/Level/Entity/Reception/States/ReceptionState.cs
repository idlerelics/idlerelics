using Game.Core;
using Injection;

namespace Game.Level.Reception
{
    public abstract class ReceptionState : State
    {
        [Inject] protected ReceptionController _reception;
    }
}

