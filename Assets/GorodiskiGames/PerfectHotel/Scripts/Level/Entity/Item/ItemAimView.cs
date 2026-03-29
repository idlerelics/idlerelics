using Game.Level.Inventory;
using UnityEngine;

namespace Game.Level.Item
{
    /// <summary>
    /// A specialized item view for items that have an aim point and a customer position.
    /// For example, a soda machine where:
    /// - TargetInventory defines what the player needs to carry (e.g., SodaCan)
    /// - AimPosition is where the player character looks/faces during interaction
    /// - CustomerPosition is where the NPC customer stands while waiting
    ///
    /// Inherits from ItemReusableView, which provides the progress bar and model binding.
    /// </summary>
    public class ItemAimView : ItemReusableView
    {
        [SerializeField] private InventoryType _targetInventory;  // What inventory item is needed here
        [SerializeField] private Transform _aimTransform;         // Where the player looks during interaction
        [SerializeField] private Transform _customerPosition;     // Where the customer NPC stands

        // Read-only properties to access the serialized data
        public InventoryType TargetInventory => _targetInventory;
        public Vector3 AimPosition => _aimTransform.position;
        public Vector3 CustomerPosition => _customerPosition.position;
    }
}
