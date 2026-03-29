using Game.Core;
using Injection;

namespace Game.Level.Cash.States
{
    /// <summary>
    /// Abstract base class for all cash/coin states (idle, flying to player, flying to pile, etc.).
    /// Provides access to the CashController via dependency injection.
    ///
    /// This follows the same pattern as other state base classes (InventoryState, PlayerState):
    /// the base class injects the controller, and subclasses implement Initialize/Dispose.
    /// </summary>
    public abstract class CashState : State
    {
        [Inject] protected CashController _cash; // The cash controller this state belongs to
    }
}
