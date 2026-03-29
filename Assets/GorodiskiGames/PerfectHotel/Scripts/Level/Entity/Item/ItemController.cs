using System;
using Core;
using Game;
using UnityEngine;
using Utilities;

namespace Game.Level.Item
{
    /// <summary>
    /// Data model for an interactive item. Inherits from Observable so the UI
    /// can watch for changes (e.g., updating a progress bar as Duration counts down).
    ///
    /// Observable is part of the Observer pattern: when SetChanged() is called,
    /// all registered observers are notified that the data has changed.
    /// </summary>
    public class ItemModel : Observable
    {
        public float Duration;         // Time remaining for the current interaction (counts down)
        public float DurationNominal;  // The full/original duration (used to calculate fill percentage)
    }

    /// <summary>
    /// Base class for all interactive items in the game world.
    /// An "item" is a trigger zone the player walks into to perform an action
    /// (cleaning, reception check-in, purchasing upgrades, etc.).
    ///
    /// Each item has a position, an interaction radius, and a type.
    /// Events notify listeners when the player interacts with or finishes the item.
    ///
    /// 'event Action' is C#'s built-in delegate event system:
    /// - Subscribe: PLAYER_ON_ITEM += MyHandler;
    /// - Unsubscribe: PLAYER_ON_ITEM -= MyHandler;
    /// - Fire: PLAYER_ON_ITEM.SafeInvoke(this);
    /// </summary>
    public class ItemController
    {
        private const int _amountDefault = 10; // Default cash amount per interaction

        /// <summary>
        /// Calculates how much cash to take for an interaction.
        /// Returns the default amount, or the player's total cash if they have less.
        /// </summary>
        public int GetAmount(long cash)
        {
            var amount = _amountDefault;

            if (amount > cash)
                amount = (int)cash; // Can't spend more than you have

            return amount;
        }

        // Events for item interactions
        public event Action<ItemController> PLAYER_ON_ITEM;           // Player is standing on this item
        public event Action<ItemController> ITEM_FINISHED;            // Player finished the interaction
        public event Action<ItemController> UNIT_LEFT_TOILET_CABINE;  // A unit left a toilet cabin

        private ItemModel _model;      // The item's data model (duration, etc.)
        private Transform _transform;  // Position in the world
        private float _radius;         // How close the player must be to interact
        private ItemType _type;        // What kind of item (Clean, ReceptionDesk, BuyUpdate, ShowHud)

        // Read-only public access via properties
        public Transform Transform => _transform;
        public float Radius => _radius;
        public ItemType Type => _type;
        public ItemModel Model => _model;
        public int Area;  // Which area/zone this item belongs to (-1 means any area)

        public ItemController(Transform transform, float radius, ItemType type)
        {
            _model = new ItemModel();

            _transform = transform;
            _radius = radius;
            _type = type;
        }

        /// <summary>Fires the ITEM_FINISHED event. 'virtual' allows subclasses to override.</summary>
        internal virtual void FireItemFinished()
        {
            ITEM_FINISHED.SafeInvoke(this);
        }

        /// <summary>Fires the PLAYER_ON_ITEM event each frame the player stands on this item.</summary>
        internal virtual void FirePlayerOnItem()
        {
            PLAYER_ON_ITEM.SafeInvoke(this);
        }

        /// <summary>Fires when a unit (NPC) leaves a toilet cabin.</summary>
        internal void FireUnitLeftToiletCabine()
        {
            UNIT_LEFT_TOILET_CABINE.SafeInvoke(this);
        }
    }

    /// <summary>
    /// An item for rooms -- has a reusable view component and belongs to a specific area.
    /// 'sealed' means no class can inherit from this one.
    /// </summary>
    public sealed class ItemRoomController : ItemController
    {
        public ItemReusableView View;

        public ItemRoomController(Transform transform, float radius, ItemType type, ItemReusableView view, int area) : base(transform, radius, type)
        {
            View = view;
            View.Model = Model; // Connect the view to the model so it can observe changes

            Area = area;
        }
    }

    /// <summary>
    /// An item for toilet cabins -- tracks how many times it has been visited
    /// and has a maximum visit count before it needs restocking.
    /// </summary>
    public sealed class ItemToiletController : ItemController
    {
        public string ID;              // Unique identifier for this toilet cabin
        public float StayDuration;     // How long a unit stays in the cabin
        public int VisitsCount;        // Current number of visits since last restock
        public int VisitsCountMax;     // Maximum visits before needing supplies

        public ItemToiletCabineView View;

        public ItemToiletController(Transform transform, float radius, ItemType type, ItemToiletCabineView view, string id, int area) : base(transform, radius, type)
        {
            View = view;
            View.Model = Model;

            ID = id;
            Area = area;
        }

        /// <summary>True if the toilet can still accept visitors (hasn't reached max visits).</summary>
        public bool IsAvailable => VisitsCount < VisitsCountMax;
    }

    /// <summary>
    /// An item for the reception desk -- has a simple (non-reusable) view.
    /// </summary>
    public sealed class ItemReceptionController : ItemController
    {
        public ItemView View;

        public ItemReceptionController(Transform transform, float radius, ItemType type, ItemView view) : base(transform, radius, type)
        {
            View = view;
            View.Model = Model;
        }
    }
}
