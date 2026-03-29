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
    public sealed class LoaderModel : EntityModel
    {
        public float Speed;
        public LoaderLvlConfig[] Lvls;

        public LoaderModel(string id, int lvl, EntityType type, LoaderConfig config) : base(id, lvl, type)
        {
            PricePurchase = config.PricePurchase;
            Lvls = config.Lvls;
            TargetPurchaseValue = config.TargetPurchaseProgress;
            Area = config.Area;

            UpdateModel();
        }

        public override void GetUpdatedValues()
        {
            TargetUpdateProgress = Lvls[LvlNext].TargetUpdateProgress;
            PriceUpdate = Lvls[LvlNext].Price;
            Speed = Lvls[Lvl].Speed;
        }

        public override int GetLvlLength()
        {
            return Lvls.Length;
        }
    }

    public sealed class LoaderController : StaffController
    {
        private readonly LoaderModel _model;
        private readonly LoaderView _view;
        private readonly LoaderUnitView _unitView;
        private readonly StateManager<LoaderState> _stateManager;
        private readonly ItemController _itemBuyUpdate;

        public LoaderView View => _view;
        public LoaderUnitView UnitView => _unitView;

        public LoaderModel Model => _model;
        public ItemController ItemBuyUpdate => _itemBuyUpdate;

        private Vector3 _initialPosition { get; }

        public override Transform InventoryHolder => UnitView.InventoryHolder;
        public override InventoryType TargetInventory => _view.TargetInventory;

        public override int Area => -1;

        public LoaderController(LoaderView view, Context context)
        {
            _view = view;
            _unitView = _view.UnitView;

            _initialPosition = _unitView.Position;

            var subContext = new Context(context);
            var injector = new Injector(subContext);

            subContext.Install(this);
            subContext.InstallByType(this, typeof(LoaderController));
            subContext.Install(injector);

            _stateManager = new StateManager<LoaderState>();
            _stateManager.IsSendLogs = false;

            injector.Inject(_stateManager);

            var gameManager = context.Get<GameManager>();
            var gameConfig = context.Get<GameConfig>();

            string id = gameManager.Model.GenerateEntityID(gameManager.Model.Hotel, _view.Type, view.Config.Number);
            _view.name = id;
            int lvl = gameManager.Model.LoadPlaceLvl(id);

            _model = new LoaderModel(id, lvl, _view.Type, _view.Config);
            _model.IsPurchased = gameManager.Model.LoadPlaceIsPurchased(id);
            _model.Cash = gameManager.Model.LoadPlaceCash(_model.ID);

            _view.HudView.Model = _model;

            _itemBuyUpdate = new ItemController(view.ItemBuyUpdateView.transform, gameConfig.BuyUpdateRadius, view.ItemBuyUpdateView.Type);
        }

        public override void Dispose()
        {
            _stateManager.Dispose();
        }

        public override void Idle()
        {
            _stateManager.SwitchToState(new LoaderIdleState());
        }

        public override void WalkToItem(Vector3 position)
        {
            _stateManager.SwitchToState(new LoaderWalkToItemState(position));
        }

        public override void WalkHome()
        {
            _stateManager.SwitchToState(new LoaderWalkHomeState(_initialPosition));
        }

        public override void ArrivedToUtility()
        {
            _stateManager.SwitchToState(new LoaderArrivedToUtilityState());
        }

        public override void WalkToUtility(Vector3 position)
        {
            _stateManager.SwitchToState(new LoaderWalkToUtilityState(position));
        }

        public override void InitializeStaff()
        {
            _stateManager.SwitchToState(new LoaderInitializeState());
        }

        public override void ReadyToPurchase()
        {
            _stateManager.SwitchToState(new LoaderReadyToPurchaseState());
        }

        public override void Hidden()
        {
            _stateManager.SwitchToState(new LoaderHiddenState());
        }
    }
}

