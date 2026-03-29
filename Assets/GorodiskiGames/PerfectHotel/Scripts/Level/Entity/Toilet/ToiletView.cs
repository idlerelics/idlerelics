using Game.Config;
using Game.Level.Item;
using Game.Level.Line;
using Game.Level.Place;
using UnityEngine;

namespace Game.Level.Toilet
{
    /// <summary>
    /// MonoBehaviour view for a toilet entity in the scene. Holds references to
    /// scene objects that the ToiletController needs: the guest queue route,
    /// the config ScriptableObject, and the individual toilet cabine item views.
    ///
    /// Extends PlaceWithCashPileView, which provides the HUD, cash pile item,
    /// and buy/update item views that all purchasable places share.
    ///
    /// SERIALIZED FIELDS: These are set in the Unity Inspector by dragging
    /// references from the scene hierarchy. [SerializeField] makes private
    /// fields visible in the Inspector while keeping them private in code.
    /// </summary>
    public sealed class ToiletView : PlaceWithCashPileView
    {
        [SerializeField] private RouteView _line;                  // The route guests follow while queuing
        [SerializeField] private ToiletConfig _config;             // ScriptableObject with balance values (price, fee, duration)
        [SerializeField] private ItemToiletCabineView[] _items;    // Array of individual toilet stall views in this toilet

        /// <summary>The route/path that defines the guest queue line.</summary>
        public RouteView Line => _line;

        /// <summary>Configuration ScriptableObject for this toilet's balance values.</summary>
        public ToiletConfig Config => _config;

        /// <summary>Array of toilet cabine (stall) views -- one per physical stall in the scene.</summary>
        public ItemToiletCabineView[] Items => _items;
    }
}
