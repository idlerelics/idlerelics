using Game.Level.Inventory;
using UnityEngine;

namespace Game.Level.Item
{
    /// <summary>
    /// FIX #8: Generalize inventory delivery via interface instead of hard casts.
    /// Previously, UtilityModule cast every DropInventory item to ItemToiletController,
    /// assuming the only delivery target was a toilet cabin. Adding a Potion Lab or any
    /// other facility that accepts deliveries would have crashed on that cast.
    ///
    /// Now UtilityModule casts to IInventoryReceiver instead. Any item controller that
    /// implements this interface works with the delivery system automatically.
    /// Currently implemented by: ItemToiletController.
    /// </summary>
    public interface IInventoryReceiver
    {
        InventoryType TargetInventory { get; }
        Vector3 AimPosition { get; }
        bool IsAvailable { get; }
    }
}
