using Game.Config;
using Game.Level.Inventory;
using Game.Level.Item;
using Game.Level.Place;
using UnityEngine;

namespace Game.Level.Loader
{
    public sealed class LoaderView : EntityWithHudView
    {
        [SerializeField] private InventoryType _targetInventory;
        [SerializeField] private LoaderConfig _config;
        [SerializeField] private LoaderUnitView _unitView;
        [SerializeField] private ItemView _itemBuyUpdateView;

        public InventoryType TargetInventory => _targetInventory;
        public LoaderConfig Config => _config;
        public LoaderUnitView UnitView => _unitView;
        public ItemView ItemBuyUpdateView => _itemBuyUpdateView;
    }
}

