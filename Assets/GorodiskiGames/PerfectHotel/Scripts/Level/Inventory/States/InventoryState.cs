using Game.Core;
using Injection;

namespace Game.Level.Inventory.InventoryStates
{
    public abstract class InventoryState : State
    {
        [Inject] protected InventoryController _inventory;
    }
}