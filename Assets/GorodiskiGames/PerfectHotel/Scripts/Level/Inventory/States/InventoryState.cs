using Game.Core;
using Injection;

namespace Game.Level.Inventory.InventoryStates
{
    /// <summary>
    /// Abstract base class for all inventory item states.
    /// 'abstract' means this class cannot be instantiated directly -- you must create
    /// a subclass (like InventoryIdleState or InventoryFlyState).
    ///
    /// The [Inject] attribute tells the DI system to automatically provide
    /// the InventoryController for this item when the state is created.
    /// 'protected' means subclasses can access this field but external code cannot.
    /// </summary>
    public abstract class InventoryState : State
    {
        [Inject] protected InventoryController _inventory;
    }
}
