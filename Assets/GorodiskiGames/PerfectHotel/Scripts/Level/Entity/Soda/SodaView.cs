using Game.Config;
using Game.Level.Item;
using Game.Level.Line;
using Game.Level.Place;
using UnityEngine;

namespace Game.Level.Soda
{
    /// <summary>
    /// MonoBehaviour view for a soda machine entity in the scene. Holds references
    /// to the guest queue route, the config ScriptableObject, and the aim item views
    /// where guests interact with the soda machine.
    ///
    /// Extends PlaceWithCashPileView, which provides the shared HUD, cash pile,
    /// and buy/update item views. This is similar to ToiletView but uses ItemAimView
    /// instead of cabine views, since soda machines have aim-based interaction points
    /// rather than enclosed stalls.
    /// </summary>
    public sealed class SodaView : PlaceWithCashPileView
    {
        [SerializeField] private RouteView _line;          // The route guests follow while queuing for the soda machine
        [SerializeField] private ToiletConfig _config;     // Config ScriptableObject (reuses ToiletConfig for similar balance fields)
        [SerializeField] private ItemAimView[] _items;     // Array of interaction points where guests use the soda machine

        /// <summary>The route/path that defines the guest queue line.</summary>
        public RouteView Line => _line;

        /// <summary>Configuration ScriptableObject with balance values (price, fee, duration).</summary>
        public ToiletConfig Config => _config;

        /// <summary>Array of aim-based interaction point views for this soda machine.</summary>
        public ItemAimView[] Items => _items;
    }
}