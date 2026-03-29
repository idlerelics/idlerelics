using System;
using System.Collections.Generic;
using Game;
using Game.Config;
using Game.Core;
using Game.Level.Entity;
using Game.Level.Item;
using Game.Level.Line;
using Game.Level.Place;
using Injection;
using UnityEngine;

namespace Game.Level.Toilet
{
    /// <summary>
    /// Data model for a toilet entity. Stores toilet-specific stats like the fee
    /// guests pay to use it and how long they stay inside a cabine.
    /// Extends EntityModel which provides the common level, purchase, and upgrade fields.
    /// </summary>
    public sealed class ToiletModel : EntityModel
    {
        public int VisualIndex;      // Which visual variant to display (different toilet skins)
        public float StayDuration;   // How long a guest occupies a cabine (in seconds)
        public int StayFee;          // How much cash a guest pays per toilet visit

        /// <summary>
        /// Constructs a new ToiletModel, pulling configuration values from a ToiletConfig
        /// ScriptableObject. The config is set up in the Unity Editor so designers can
        /// tweak balance values without touching code.
        /// </summary>
        public ToiletModel(string id, int lvl, EntityType type, ToiletConfig config, int visualIndex) : base(id, lvl, type)
        {
            VisualIndex = visualIndex;
            PricePurchase = config.PricePurchase;
            StayFee = config.StayFee;
            StayDuration = config.StayDuration;
            TargetPurchaseValue = config.TargetPurchaseProgress;
            PurchaseProgressReward = config.PurchaseProgressReward;
            Area = config.Area;

            UpdateModel(); // Calculate derived values based on the current level
        }

        /// <summary>Returns 0 because the toilet currently has no level upgrades.</summary>
        public override int GetLvlLength()
        {
            return 0;
        }

        /// <summary>
        /// No level-specific values to recalculate for toilets (no upgrade tiers).
        /// This is required by the abstract base class but left empty.
        /// </summary>
        public override void GetUpdatedValues()
        {

        }
    }

    /// <summary>
    /// Controller for a toilet facility. Manages the toilet's state machine, cabines
    /// (individual stalls), cash pile, guest queue (line), and upgrade interactions.
    ///
    /// IDisposable is implemented to clean up the state machine when the toilet
    /// is destroyed, preventing memory leaks from lingering event subscriptions.
    ///
    /// ARCHITECTURE: Each toilet gets its own sub-Context (child DI container)
    /// so its states can inject toilet-specific dependencies without polluting
    /// the parent context.
    /// </summary>
    public sealed class ToiletController : EntityController, IDisposable
    {
        private readonly ToiletModel _model;
        private readonly ToiletView _view;
        private readonly StateManager<ToiletState> _stateManager;          // Manages the toilet's state transitions
        private readonly Dictionary<ItemToiletController, bool> _cabinesMap; // Maps each cabine to its availability (true = free)
        private readonly ItemController _itemBuyUpdate;                     // Trigger zone for upgrading the toilet
        private readonly ItemController _itemCashPile;                      // Trigger zone for collecting earned cash
        private readonly LineController _line;                              // Queue of guests waiting for a free cabine

        // Public read-only accessors -- expose internals to states without allowing modification
        public ToiletModel Model => _model;
        public ToiletView View => _view;
        public ItemController ItemBuyUpdate => _itemBuyUpdate;
        public ItemController ItemCashPile => _itemCashPile;
        public Dictionary<ItemToiletController, bool> CabinesMap => _cabinesMap;
        public LineController Line => _line;

        /// <summary>Returns the world-space transform of this toilet's view (used for positioning).</summary>
        public override Transform Transform => _view.transform;

        /// <summary>
        /// Constructs the toilet controller: sets up the DI sub-context, initializes the model
        /// from saved data, creates cabine controllers for each stall, and enters the initial state.
        /// </summary>
        /// <param name="view">The MonoBehaviour view placed in the scene.</param>
        /// <param name="context">The parent DI context to create a child context from.</param>
        public ToiletController(ToiletView view, Context context)
        {
            _view = view;

            // CameraAngleSign determines which side the camera approaches from when focusing
            CameraAngleSign = _view.CameraAngleSign;

            // Create a child DI context so toilet-specific dependencies don't leak into the global scope
            var subContext = new Context(context);
            var injector = new Injector(subContext);

            // Register this controller in the sub-context so toilet states can inject it
            subContext.Install(this);
            subContext.InstallByType(this, typeof(ToiletController));
            subContext.Install(injector);

            // Create the state machine and inject its dependencies
            _stateManager = new StateManager<ToiletState>();
            _stateManager.IsSendLogs = false; // Disable debug logging for performance

            injector.Inject(_stateManager);

            // Pull shared dependencies from the parent context
            var gameManager = context.Get<GameManager>();
            var gameConfig = context.Get<GameConfig>();

            // Generate a unique ID for this toilet based on hotel number, entity type, and config number
            string id = gameManager.Model.GenerateEntityID(gameManager.Model.Hotel, _view.Type, view.Config.Number);
            _view.name = id; // Name the GameObject for easier debugging in the Unity hierarchy
            int lvl = gameManager.Model.LoadPlaceLvl(id);
            int visualIndex = gameManager.Model.LoadPlaceVisualIndex(id);

            // Create the model and restore persisted state
            _model = new ToiletModel(id, lvl, _view.Type, _view.Config, visualIndex);
            _model.IsPurchased = gameManager.Model.LoadPlaceIsPurchased(id);
            _model.Cash = gameManager.Model.LoadPlaceCash(_model.ID);

            // Link the model to the HUD so it can display price/level info
            _view.HudView.Model = _model;

            // Initialize the cabine map -- each cabine is an individual toilet stall
            _cabinesMap = new Dictionary<ItemToiletController, bool>();
            int cabineNumber = 0;
            foreach (var itemView in _view.Items)
            {
                // Each cabine gets a unique ID derived from the toilet's ID + type + index
                string cabineID = _model.ID + itemView.Type + cabineNumber;
                var cabine = new ItemToiletController(itemView.transform, gameConfig.ToiletItemRadius, itemView.Type, itemView, cabineID, _model.Area);

                // Copy toilet-level settings to each cabine
                cabine.StayDuration = _model.StayDuration;
                cabine.VisitsCountMax = gameConfig.ToiletVisitsCountMax;
                cabine.VisitsCount = gameManager.Model.LoadVisitsCount(cabine.ID);

                // Track cabine availability (true if the stall is free for a guest)
                _cabinesMap.Add(cabine, cabine.IsAvailable);

                cabineNumber++;
            }

            // Create item controllers for the cash pile and buy/upgrade trigger zones
            _itemCashPile = new ItemController(view.ItemCashPileView.transform, gameConfig.CashPileRadius, view.ItemCashPileView.Type);
            _itemBuyUpdate = new ItemController(view.ItemBuyUpdateView.transform, gameConfig.BuyUpdateRadius, view.ItemBuyUpdateView.Type);

            // Initialize the guest queue line
            _line = new LineController(_view.Line);

            // Enter the initial state (which decides if the toilet is purchased, hidden, etc.)
            SwitchToState(new ToiletInitializeState());
        }

        /// <summary>
        /// Transitions the toilet to a new state. The state machine handles calling
        /// Dispose() on the old state and Initialize() on the new one.
        /// </summary>
        /// <typeparam name="T">A type that extends ToiletState.</typeparam>
        /// <param name="instance">The new state instance to switch to.</param>
        public void SwitchToState<T>(T instance) where T : ToiletState
        {
            _stateManager.SwitchToState(instance);
        }

        /// <summary>
        /// Cleans up the state machine, which in turn disposes the current state
        /// and unsubscribes from any active events.
        /// </summary>
        public void Dispose()
        {
            _stateManager.Dispose();
        }

        /// <summary>
        /// Returns the progress reward granted when this toilet is purchased.
        /// Used by the game manager to advance overall game progress.
        /// </summary>
        public int GetTotalReward()
        {
            return _model.PurchaseProgressReward;
        }

        /// <summary>
        /// Searches through all cabines and returns the first one that is available (free).
        /// Returns null if all cabines are occupied -- the guest will need to wait in line.
        /// </summary>
        public ItemController GetAvailableCabine()
        {
            foreach (var cabine in _cabinesMap.Keys)
            {
                bool isAvailable = _cabinesMap[cabine];
                if (isAvailable)
                {
                    return cabine;
                }
            }
            return null;
        }
    }
}
