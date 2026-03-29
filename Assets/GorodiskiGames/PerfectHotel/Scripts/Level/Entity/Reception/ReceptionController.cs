using Game.Core;
using Injection;
using System.Collections.Generic;
using Game.Config;
using UnityEngine;
using Game.Level.Entity;
using Game.Level.Place;
using Game.Level.Item;
using Game.Level.Line;

namespace Game.Level.Reception
{
    /// <summary>
    /// Data model for the reception desk. Tracks receptionist count, guest processing time,
    /// and upgrade levels.
    ///
    /// Inherits from EntityModel which provides common entity data (ID, level, cash, etc.).
    /// </summary>
    public sealed class ReceptionModel : EntityModel
    {
        public int ReceptionistCount;               // How many receptionists are working
        public float UnitProceedTime = 1f;           // Time to process each guest (seconds)

        public ReceptionLvlConfig[] Lvls;            // Stats for each upgrade level
        public int ItemsToShow = 1;                  // How many service items are visible

        public ReceptionModel(string id, int lvl, EntityType type, ReceptionConfig config) : base(id, lvl, type)
        {
            Lvls = config.Lvls;

            UpdateModel(); // Calculate current stats based on level
        }

        /// <summary>Returns the total number of upgrade levels available.</summary>
        public override int GetLvlLength()
        {
            return Lvls.Length;
        }

        /// <summary>Updates model values based on the current and next upgrade level.</summary>
        public override void GetUpdatedValues()
        {
            TargetUpdateProgress = Lvls[LvlNext].TargetUpdateProgress;
            PriceUpdate = Lvls[LvlNext].Price;
            ReceptionistCount = Lvls[Lvl].ReceptionistCount;
        }
    }

    /// <summary>
    /// Controller for the reception desk entity.
    /// Manages the guest queue (line), cash pile, upgrade logic, and state transitions.
    ///
    /// Sets up its own DI sub-context and creates interactive items for:
    /// - Each receptionist desk position
    /// - The cash pile (where earned cash accumulates)
    /// - The buy/upgrade button area
    ///
    /// Inherits from EntityController, which provides the base for all purchasable/upgradeable entities.
    /// </summary>
    public sealed class ReceptionController : EntityController
    {
        private readonly ReceptionModel _model;
        private readonly ReceptionView _view;
        private readonly StateManager<ReceptionState> _stateManager;
        private readonly List<ItemController> _items;        // Receptionist desk interaction items
        private readonly ItemController _itemCashPile;       // Where cash piles up
        private readonly ItemController _itemBuyUpdate;      // Where the player stands to upgrade
        private readonly LineController _line;               // Guest queue management

        public ReceptionView View => _view;
        public ReceptionModel Model => _model;
        public List<ItemController> Items => _items;
        public ItemController ItemCashPile => _itemCashPile;
        public ItemController ItemBuyUpdate => _itemBuyUpdate;
        public LineController Line => _line;

        public override Transform Transform => _view.transform;

        public ReceptionController(ReceptionView view, Context context)
        {
            _view = view;

            // Create a child DI context for this reception's dependencies
            var subContext = new Context(context);
            var injector = new Injector(subContext);

            subContext.Install(this);
            subContext.Install(injector);

            _stateManager = new StateManager<ReceptionState>();
            _stateManager.IsSendLogs = false; // Disable state change logging for performance

            injector.Inject(_stateManager);

            var gameManager = context.Get<GameManager>();
            var gameConfig = context.Get<GameConfig>();

            // Generate a unique ID for save/load based on hotel, entity type, and index
            string id = gameManager.Model.GenerateEntityID(gameManager.Model.Hotel, _view.Type, 0);
            int lvl = gameManager.Model.LoadPlaceLvl(id); // Load saved upgrade level

            _model = new ReceptionModel(id, lvl, _view.Type, _view.Config);
            _model.IsPurchased = true; // Reception is always purchased (always present in the level)

            // Load saved cash from persistent storage
            var cash = gameManager.Model.LoadPlaceCash(_model.ID);
            _model.Cash = cash;

            // Connect the HUD view to the model for automatic UI updates
            _view.HudView.Model = _model;

            // Create interactive items for each receptionist desk
            _items = new List<ItemController>();
            foreach (var itemView in _view.Items)
            {
                var item = new ItemReceptionController(itemView.transform, gameConfig.ReceptionItemRadius, itemView.Type, itemView);
                _items.Add(item);
            }

            // Create the cash pile and upgrade items
            _itemCashPile = new ItemController(view.ItemCashPileView.transform, gameConfig.CashPileRadius, view.ItemCashPileView.Type);
            _itemBuyUpdate = new ItemController(view.ItemBuyUpdateView.transform, gameConfig.BuyUpdateRadius, view.ItemBuyUpdateView.Type);

            // Set up the guest queue
            _line = new LineController(_view.Line);

            // Start in the idle state
            SwitchToState(new ReceptionIdleState());
        }

        /// <summary>Transitions the reception to a new state.</summary>
        public void SwitchToState<T>(T instance) where T : ReceptionState
        {
            _stateManager.SwitchToState(instance);
        }

        public void Dispose()
        {
            _stateManager.Dispose();
        }
    }
}
