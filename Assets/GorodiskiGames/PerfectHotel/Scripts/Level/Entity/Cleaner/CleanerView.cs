using Game.Config;
using Game.Level.Item;
using Game.Level.Place;
using UnityEngine;

namespace Game.Level.Cleaner
{
    public sealed class CleanerView : EntityWithHudView
    {
        [SerializeField] private ItemType _targetItem;
        [SerializeField] private CleanerConfig _config;
        [SerializeField] private CleanerUnitView _unitView;
        [SerializeField] private ItemView _itemBuyUpdateView;

        public ItemType TargetItem => _targetItem;
        public CleanerConfig Config => _config;
        public CleanerUnitView UnitView => _unitView;
        public ItemView ItemBuyUpdateView => _itemBuyUpdateView;
    }
}

