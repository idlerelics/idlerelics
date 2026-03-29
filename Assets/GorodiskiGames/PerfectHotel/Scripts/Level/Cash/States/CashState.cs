using Game.Core;
using Injection;

namespace Game.Level.Cash.States
{
    public abstract class CashState : State
    {
        [Inject] protected CashController _cash;
    }
}