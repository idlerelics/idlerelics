using Game.Config;
using Game.Level.Item;
using Game.Level.Line;
using Game.Level.Place;
using UnityEngine;

namespace Game.Level.Toilet
{
    public sealed class ToiletView : PlaceWithCashPileView
    {
        [SerializeField] private RouteView _line;
        [SerializeField] private ToiletConfig _config;
        [SerializeField] private ItemToiletCabineView[] _items;

        public RouteView Line => _line;
        public ToiletConfig Config => _config;
        public ItemToiletCabineView[] Items => _items;
    }
}

