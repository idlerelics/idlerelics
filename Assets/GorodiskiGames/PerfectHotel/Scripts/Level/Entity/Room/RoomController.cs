using Game.Core;
using Injection;
using Game.Config;
using Game;
using Core;
using System.Collections.Generic;
using System;
using UnityEngine;
using Game.Level.Area;
using Game.Level.Place;
using Game.Level.Entity;
using Game.Level.Item;

namespace Game.Level.Entity
{
    /// <summary>
    /// Base data model for all game entities (rooms, reception, toilets, elevators, etc.).
    /// Tracks level, purchase state, upgrade prices, and cash earnings.
    /// Extends Observable so the UI can listen for changes and update automatically
    /// (Observer pattern: when data changes, all "observers" are notified).
    /// </summary>
    public abstract class EntityModel : Observable
    {
        public EntityType Type;   // What kind of entity this is (Room, Elevator, etc.)
        public string ID;         // Unique identifier string (used for saving/loading)

        public long Cash;         // How much cash this entity has earned (waiting to be collected)

        public int Lvl;           // Current upgrade level
        public int LvlNext;      // The next level to upgrade to

        public bool IsPurchased { get; internal set; }  // Has this entity been bought?
        public bool IsMaxed { get; internal set; }      // Is it at maximum level?
        public bool IsLocked { get; internal set; }     // Is it locked (cannot interact)?

        public int PricePurchase;           // Cost to buy this entity
        public int PriceUpdate;             // Cost to upgrade to the next level

        public int Area;                    // Which area/floor this entity belongs to
        public int TargetPurchaseValue;     // Progress needed to unlock purchase
        public int TargetUpdateProgress;    // Progress needed to unlock upgrade

        public int PurchaseProgressReward;  // Reward given when purchased
        public int UpdateProgressReward;    // Reward given when upgraded

        // FIX #6: Data-driven HUD labels instead of hardcoded if/else in EntityHudView.
        // Previously EntityHudView checked EntityType to decide what text to show ("NEW AREA",
        // "RECEPTIONIST", etc.). Now the labels live on the model — EntityHudView just reads them.
        // New entity types get a sensible default (type name) or can set custom labels here.
        public string PurchaseLabel;       // Label shown on HUD when ready to purchase
        public string UpgradeLabel;        // Label shown on HUD when upgrading (null = use "LVL X")

        public EntityModel(string id, int lvl, EntityType type)
        {
            ID = id;
            Lvl = lvl;
            Type = type;

            // Default labels — override PurchaseLabel/UpgradeLabel after construction for custom text
            PurchaseLabel = type.ToString().ToUpper();

            if (type == EntityType.Area) PurchaseLabel = "NEW AREA";
            else if (type == EntityType.Elevator) PurchaseLabel = "NEW HOTEL";

            if (type == EntityType.Reception) UpgradeLabel = "RECEPTIONIST";
            else if (type == EntityType.Cleaner) UpgradeLabel = "SPEED";
        }

        /// <summary>Returns the total number of levels this entity can have (defined by subclass).</summary>
        public abstract int GetLvlLength();

        /// <summary>
        /// Recalculates model values after a level change.
        /// "virtual" means subclasses CAN override this, but don't have to (unlike "abstract").
        /// </summary>
        public virtual void UpdateModel()
        {
            LvlNext = Lvl + 1;
            int lvlLength = GetLvlLength();
            if (LvlNext >= lvlLength) // If we're at or past the last level, mark as maxed
            {
                LvlNext = Lvl;
                IsMaxed = true;
            }
            GetUpdatedValues(); // Let the subclass fill in level-specific values
        }

        /// <summary>Subclasses must implement this to set values based on the current level.</summary>
        public abstract void GetUpdatedValues();
    }

    /// <summary>
    /// Base controller for entities that exist in the game world (have a Transform/position).
    /// CameraAngleSign controls which direction the camera rotates when focusing on this entity.
    /// </summary>
    public abstract class EntityController : EntityUpdatableController
    {
        public int CameraAngleSign;               // -1, 0, or 1 for camera rotation direction
        public abstract Transform Transform { get; } // The entity's position in the world
    }

    /// <summary>
    /// Base class with helper methods to check if an entity can be upgraded or purchased.
    /// Both PlayerController and EntityController inherit from this.
    /// </summary>
    public abstract class EntityUpdatableController
    {
        /// <summary>Returns true if the entity can be upgraded (not maxed and enough progress).</summary>
        public bool IsUpdatable(bool isMaxed, int currentProgress, int targetUpdateProgress)
        {
            return !isMaxed && currentProgress >= targetUpdateProgress;
        }

        /// <summary>Returns true if the entity can be purchased (area is open and enough resources).</summary>
        public bool IsPurchasable(bool isAreaPurchased, int currentValue, int targetValue)
        {
            return isAreaPurchased && currentValue >= targetValue;
        }
    }
}

namespace Game.Level.Room
{
    /// <summary>
    /// Data model for a hotel room. Stores room-specific values like fees, cleaning time,
    /// and the array of level configs that define how the room improves with each upgrade.
    /// </summary>
    public sealed class RoomModel : EntityModel
    {
        public int VisualIndex;       // Which visual variant to display (different furniture, etc.)
        public float CleaningTime;    // How long it takes to clean this room (in seconds)
        public int StayFee;           // How much cash a guest pays to stay
        public float StayDuration;    // How long a guest stays (in seconds)
        public int EntranceFee;       // Fee charged when a guest enters

        public RoomLvlConfig[] Lvls;  // Array of configs, one per upgrade level

        /// <summary>
        /// Constructor: builds a RoomModel from config data and immediately calculates
        /// its current-level values via UpdateModel().
        /// </summary>
        public RoomModel(string id, int lvl, EntityType type, RoomConfig config, int visualIndex) : base(id, lvl, type)
        {
            VisualIndex = visualIndex;
            StayDuration = config.StayDuration;
            TargetPurchaseValue = config.TargetPurchaseProgress;
            PricePurchase = config.PricePurchase;
            PurchaseProgressReward = config.PurchaseProgressReward;
            UpdateProgressReward = config.UpdateProgressReward;
            Area = config.Area;

            Lvls = config.Lvls;

            UpdateModel();
        }

        /// <summary>Sets room values (fees, cleaning time, excavation duration) based on the current level.</summary>
        public override void GetUpdatedValues()
        {
            TargetUpdateProgress = Lvls[LvlNext].TargetUpdateProgress;
            PriceUpdate = Lvls[LvlNext].PriceUpdate;
            CleaningTime = Lvls[Lvl].CleaningTime;
            StayFee = Lvls[Lvl].StayFee;
            EntranceFee = Lvls[Lvl].EntranceFee;

            // Per-level StayDuration: use level config if set, otherwise keep the fallback from RoomConfig
            if (Lvls[Lvl].StayDuration > 0f)
                StayDuration = Lvls[Lvl].StayDuration;
        }

        public override int GetLvlLength()
        {
            return Lvls.Length;
        }
    }



    /// <summary>
    /// The main controller for a hotel room. Manages the room's state machine,
    /// its items (bed, desk, etc.), its cash pile, and its buy/upgrade button.
    /// Implements IDisposable for cleanup when the room is destroyed.
    /// </summary>
    public sealed class RoomController : EntityController, IDisposable
    {
        private readonly RoomModel _model;                      // Room's data (fees, level, etc.)
        private readonly RoomView _view;                        // Room's visual representation in the scene
        private readonly StateManager<RoomState> _stateManager; // State machine (available, occupied, dirty, etc.)

        private readonly List<ItemRoomController> _items;       // Interactive items inside the room

        private readonly ItemController _itemCashPile;    // The cash pile item where earned money appears
        private readonly ItemController _itemBuyUpdate;   // The buy/upgrade button item

        public RoomView View => _view;
        public RoomModel Model => _model;

        public List<ItemRoomController> Items => _items;

        public ItemController ItemCashPile => _itemCashPile;
        public ItemController ItemBuyUpdate => _itemBuyUpdate;

        public Action ON_EXCAVATION_COMPLETE;  // Fired when excavation timer ends, unit should leave

        public AreaController Area { get; internal set; }
        public bool IsAvailable { get; internal set; }

        public override Transform Transform => _view.transform;

        /// <summary>
        /// Constructor: builds the room from its view and context.
        /// 1. Sets up dependency injection
        /// 2. Loads saved data (level, purchase state, cash) from PlayerPrefs
        /// 3. Creates the data model
        /// 4. Creates item controllers for interactive objects in the room
        /// 5. Starts the room's state machine
        /// </summary>
        public RoomController(RoomView view, Context context)
        {
            _view = view;

            CameraAngleSign = _view.CameraAngleSign;

            // Set up dependency injection for this room's state machine
            var subContext = new Context(context);
            var injector = new Injector(subContext);

            subContext.Install(this);
            subContext.Install(injector);

            _stateManager = new StateManager<RoomState>();
            _stateManager.IsSendLogs = false;

            injector.Inject(_stateManager);

            // Get references to global systems from the DI context
            var gameManager = context.Get<GameManager>();
            var gameConfig = context.Get<GameConfig>();

            // Generate a unique ID for this room and load its saved state
            string id = gameManager.Model.GenerateEntityID(gameManager.Model.Hotel, _view.Type, view.Config.Number);
            _view.name = id; // Name the GameObject for easier debugging in the Unity Hierarchy
            int lvl = gameManager.Model.LoadPlaceLvl(id);
            int visualIndex = gameManager.Model.LoadPlaceVisualIndex(id);

            // Create the data model with loaded values
            _model = new RoomModel(id, lvl, _view.Type, _view.Config, visualIndex);
            _model.IsPurchased = gameManager.Model.LoadPlaceIsPurchased(id);
            _model.Cash = gameManager.Model.LoadPlaceCash(_model.ID);

            _view.HudView.Model = _model; // Connect the HUD (UI above the room) to the data

            // Create controllers for each interactive item in the room (bed, desk, etc.)
            _items = new List<ItemRoomController>();
            foreach (var itemView in _view.Items)
            {
                var item = new ItemRoomController(itemView.transform, gameConfig.RoomItemRadius, itemView.Type, itemView, _view.Config.Area);
                _items.Add(item);
            }

            // Create controllers for the cash pile and buy/upgrade button
            _itemCashPile = new ItemController(_view.ItemCashPileView.transform, gameConfig.CashPileRadius, _view.ItemCashPileView.Type);
            _itemBuyUpdate = new ItemController(_view.ItemBuyUpdateView.transform, gameConfig.BuyUpdateRadius, _view.ItemBuyUpdateView.Type);

            // Start the room in its initialize state (the state machine takes over from here)
            SwitchToState(new RoomInitializeState());
        }

        /// <summary>Switches the room to a new state (e.g., Available, Occupied, Dirty).</summary>
        public void SwitchToState<T>(T instance) where T : RoomState
        {
            _stateManager.SwitchToState(instance);
        }

        /// <summary>Cleans up the state machine when this room is destroyed.</summary>
        public void Dispose()
        {
            _stateManager.Dispose();
        }

        /// <summary>
        /// Calculates the total reward points this room can generate across all its levels.
        /// Used by the level view to track overall progress.
        /// </summary>
        public int GetTotalReward()
        {
            int lvlCount = _model.Lvls.Length - 1;
            int purchaseReward = _model.PurchaseProgressReward;
            int updateReward = _model.UpdateProgressReward;
            int reward = purchaseReward + (lvlCount * updateReward);
            return reward;
        }
    }
}






