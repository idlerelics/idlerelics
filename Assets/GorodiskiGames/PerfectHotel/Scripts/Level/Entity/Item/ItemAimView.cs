using Game.Level.Inventory;
using UnityEngine;

namespace Game.Level.Item
{
    public class ItemAimView : ItemReusableView
    {
        [SerializeField] private InventoryType _targetInventory;
        [SerializeField] private Transform _aimTransform;
        [SerializeField] private Transform _customerPosition;

        public InventoryType TargetInventory => _targetInventory;
        public Vector3 AimPosition => _aimTransform.position;
        public Vector3 CustomerPosition => _customerPosition.position;
    }
}

