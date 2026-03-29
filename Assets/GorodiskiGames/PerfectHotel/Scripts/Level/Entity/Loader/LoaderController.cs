using Game.Config;
using Game.Core;
using Game.Level.Entity;
using Game.Level.Inventory;
using Game.Level.Item;
using Game.Level.Loader.LoaderStates;
using Game.Level.Place;
using Game.Level.Player;
using Injection;
using UnityEngine;

namespace Game.Level.Loader
{
    /// <summary>
    /// Data model for a loader (staff member that carries items between utilities).
    /// Stores loader-specific stats like walk speed and level configuration tiers.
    /// Each level tier defines a new upgrade price, progress requirement, and speed.
    /// </summary>
    public sealed class LoaderModel : EntityModel
    {
        public float Speed;              // Current walk speed (determined by level)
        public LoaderLvlConfig[] Lvls;   // Array of per-level configs (price, speed, progress targets)

        /// <summary>
        /// Constructs a LoaderModel from a LoaderConfig ScriptableObject.
        /// The config contains all balance data set by designers in the Unity Editor.
        /// </summary>
        public LoaderModel(string id, int lvl, EntityType type, LoaderConfig config) : base(id, lvl, type)
        {
            PricePurchase = config.PricePurchase;
            Lvls = config.Lvls;
            TargetPurchaseValue = config.TargetPurchaseProgress;
            Area = config.Area;

            UpdateModel(); // Calculate derived values (speed, prices) for the current level
        }

        /// <summary>
        /// Updates level-specific values when the loader levels up.
        /// Reads the next level's upgrade requirements and the current level's speed.
        /// </summary>
        public override void GetUpdatedValues()
        {
            TargetUpdateProgress = Lvls[LvlNext].TargetUpdateProgress;
            PriceUpdate = Lvls[LvlNext].Price;
            Speed = Lvls[Lvl].Speed;
        }

        /// <summary>Returns the total number of loader levels (used to check if maxed).</summary>
        public override int GetLvlLength()
        {
            return Lvls.Length;
        }
    }

    /// <summary>
    /// Controller for a loader staff member -- an NPC that automatically carries
    /// items from pickup points to utility stations. The loader uses a state machine
    /// to cycle through behaviors: Idle -> WalkToItem -> WalkToUtility -> ArrivedToUtility -> Idle.
    ///
    /// Extends StaffController, which provides shared staff functionality like
    /// inventory management, walk commands, and home position tracking.
    ///
    /// ARCHITECTURE: Like other entity controllers, the loader creates its own
    /// sub-Context (child DI container) so its states can inject loader-specific
    /// dependencies without affecting the global scope.
    /// </summary>
    public sealed class LoaderController : StaffController
    {
        private readonly LoaderModel _model;
        private readonly LoaderView _view;
        private readonly LoaderUnitView _unitView;                          // The visual unit (3D character) for this loader
        private readonly StateManager<LoaderState> _stateManager;           // Manages state transitions for the loader
        private readonly ItemController _itemBuyUpdate;                     // Trigger zone for upgrading the loader

        /// <summary>The scene view for this loader (placement, config, HUD references).</summary>
        public LoaderView View => _view;

        /// <summary>The visual unit view (the actual 3D character walking around).</summary>
        public LoaderUnitView UnitView => _unitView;

        /// <summary>The loader's data model (level, speed, prices).</summary>
        public LoaderModel Model => _model;

        /// <summary>The item controller for the upgrade trigger zone.</summary>
        public ItemController ItemBuyUpdate => _itemBuyUpdate;

        /// <summary>The loader's starting position (where it returns when idle).</summary>
        private Vector3 _initialPosition { get; }

        /// <summary>The transform where carried items are visually parented.</summary>
        public override Transform InventoryHolder => UnitView.InventoryHolder;

        /// <summary>The type of inventory this loader targets (what items it picks up).</summary>
        public override InventoryType TargetInventory => _view.TargetInventory;

        /// <summary>Returns -1 because loaders are not bound to a specific area (they roam freely).</summary>
        public override int Area => -1;

        /// <summary>
        /// Constructs the loader controller: creates a child DI context, initializes the
        /// model from saved data, and creates the upgrade item controller.
        /// Note: the initial state is NOT set here -- InitializeStaff() must be called separately
        /// to enter the state machine, unlike most other entity controllers.
        /// </summary>
        /// <param name="view">The MonoBehaviour view placed in the scene.</param>
        /// <param name="context">The parent DI context to create a child context from.</param>
        public LoaderController(LoaderView view, Context context)
        {
            _view = view;
            _unitView = _view.UnitView;

            // Remember the starting position so the loader can return home after completing a task
            _initialPosition = _unitView.Position;

            // Create a child DI context for loader-specific dependencies
            var subContext = new Context(context);
            var injector = new Injector(subContext);

            // Register this controller in the sub-context so loader states can inject it
            subContext.Install(this);
            subContext.InstallByType(this, typeof(LoaderController));
            subContext.Install(injector);

            // Create and configure the state machine
            _stateManager = new StateManager<LoaderState>();
            _stateManager.IsSendLogs = false; // Disable debug logging for performance

            injector.Inject(_stateManager);

            // Pull shared dependencies from the parent context
            var gameManager = context.Get<GameManager>();
            var gameConfig = context.Get<GameConfig>();

            // Generate a unique ID and load persisted state
            string id = gameManager.Model.GenerateEntityID(gameManager.Model.Hotel, _view.Type, view.Config.Number);
            _view.name = id; // Name the GameObject for easier debugging in the hierarchy
            int lvl = gameManager.Model.LoadPlaceLvl(id);

            // Create the model and restore saved data
            _model = new LoaderModel(id, lvl, _view.Type, _view.Config);
            _model.IsPurchased = gameManager.Model.LoadPlaceIsPurchased(id);
            _model.Cash = gameManager.Model.LoadPlaceCash(_model.ID);

            // Link the model to the HUD for automatic display updates
            _view.HudView.Model = _model;

            // Create the upgrade trigger zone item
            _itemBuyUpdate = new ItemController(view.ItemBuyUpdateView.transform, gameConfig.BuyUpdateRadius, view.ItemBuyUpdateView.Type);
        }

        /// <summary>
        /// Cleans up the state machine, disposing the current state and
        /// unsubscribing from any active events.
        /// </summary>
        public override void Dispose()
        {
            _stateManager.Dispose();
        }

        /// <summary>Transitions the loader to the idle state (standing at home position).</summary>
        public override void Idle()
        {
            _stateManager.SwitchToState(new LoaderIdleState());
        }

        /// <summary>
        /// Commands the loader to walk toward an item pickup position.
        /// </summary>
        /// <param name="position">The world-space position of the item to pick up.</param>
        public override void WalkToItem(Vector3 position)
        {
            _stateManager.SwitchToState(new LoaderWalkToItemState(position));
        }

        /// <summary>Commands the loader to walk back to its initial/home position.</summary>
        public override void WalkHome()
        {
            _stateManager.SwitchToState(new LoaderWalkHomeState(_initialPosition));
        }

        /// <summary>
        /// Called when the loader has arrived at the utility station.
        /// Transitions to the arrival state where items are delivered.
        /// </summary>
        public override void ArrivedToUtility()
        {
            _stateManager.SwitchToState(new LoaderArrivedToUtilityState());
        }

        /// <summary>
        /// Commands the loader to walk toward a utility station to deliver items.
        /// </summary>
        /// <param name="position">The world-space position of the target utility.</param>
        public override void WalkToUtility(Vector3 position)
        {
            _stateManager.SwitchToState(new LoaderWalkToUtilityState(position));
        }

        /// <summary>
        /// Entry point for the loader's state machine. Called after construction
        /// to determine the initial state (purchased, hidden, ready to purchase, etc.).
        /// </summary>
        public override void InitializeStaff()
        {
            _stateManager.SwitchToState(new LoaderInitializeState());
        }

        /// <summary>Transitions to the ready-to-purchase state (player can buy this loader).</summary>
        public override void ReadyToPurchase()
        {
            _stateManager.SwitchToState(new LoaderReadyToPurchaseState());
        }

        /// <summary>Transitions to the hidden state (loader is not yet unlocked).</summary>
        public override void Hidden()
        {
            _stateManager.SwitchToState(new LoaderHiddenState());
        }
    }
}
