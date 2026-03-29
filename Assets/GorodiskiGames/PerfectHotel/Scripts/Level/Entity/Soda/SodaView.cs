using Game.Config;
using Game.Level.Item;
using Game.Level.Line;
using Game.Level.Place;
using UnityEngine;

namespace Game.Level.Soda
{
    public sealed class SodaView : PlaceWithCashPileView
    {
        [SerializeField] private RouteView _line;
        [SerializeField] private ToiletConfig _config;
        [SerializeField] private ItemAimView[] _items;

        public RouteView Line => _line;
        public ToiletConfig Config => _config;
        public ItemAimView[] Items => _items;
    }
}