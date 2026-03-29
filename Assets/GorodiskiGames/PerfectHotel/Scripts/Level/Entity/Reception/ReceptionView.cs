using Game.Config;
using Game.Level.Cash;
using Game.Level.Item;
using Game.Level.Line;
using Game.Level.Place;
using UnityEngine;

namespace Game.Level.Reception
{
    public sealed class ReceptionView : PlaceView
    {
        [SerializeField] private RouteView _line;
        [SerializeField] private ReceptionConfig _config;
        [SerializeField] private ItemFillBarView[] _items;
        [SerializeField] private CashPileView _cashPileView;
        [SerializeField] private ItemView _itemCashPileView;

        public ItemFillBarView[] Items => _items;
        public RouteView Line => _line;
        public ReceptionConfig Config => _config;
        public CashPileView CashPileView => _cashPileView;
        public ItemView ItemCashPileView => _itemCashPileView;
    }
}

