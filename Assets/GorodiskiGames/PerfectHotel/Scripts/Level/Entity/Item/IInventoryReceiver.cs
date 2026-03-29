using Game.Level.Inventory;
using UnityEngine;

namespace Game.Level.Item
{
    /// <summary>
    /// Interface for any item controller that can receive inventory deliveries
    /// from staff (e.g., toilet cabins needing toilet paper, potion labs needing ingredients).
    /// Implement this on new item controllers to make them work with the UtilityModule
    /// delivery system without modifying UtilityModule code.
    /// </summary>
    public interface IInventoryReceiver
    {
        InventoryType TargetInventory { get; }
        Vector3 AimPosition { get; }
        bool IsAvailable { get; }
    }
}
