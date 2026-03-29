using Game.Config;
using Game.Level.Cash;
using Game.Level.Item;
using Game.Level.Line;
using Game.Level.Place;
using UnityEngine;

namespace Game.Level.Reception
{
    /// <summary>
    /// Unity view component for the reception desk.
    /// Holds serialized references to the reception's child objects
    /// (queue line, config, interaction items, cash pile).
    ///
    /// Inherits from PlaceView which provides the base HUD view, entity type, and upgrade item.
    ///
    /// All references are set in the Unity Inspector by dragging the appropriate
    /// GameObjects/Components onto these fields.
    /// </summary>
    public sealed class ReceptionView : PlaceView
    {
        [SerializeField] private RouteView _line;              // The guest queue waypoints
        [SerializeField] private ReceptionConfig _config;      // Balance/stats configuration
        [SerializeField] private ItemFillBarView[] _items;     // Receptionist desk interaction items
        [SerializeField] private CashPileView _cashPileView;   // Where cash visually piles up
        [SerializeField] private ItemView _itemCashPileView;   // The cash pile interaction trigger

        // Read-only properties for the controller to access
        public ItemFillBarView[] Items => _items;
        public RouteView Line => _line;
        public ReceptionConfig Config => _config;
        public CashPileView CashPileView => _cashPileView;
        public ItemView ItemCashPileView => _itemCashPileView;
    }
}
