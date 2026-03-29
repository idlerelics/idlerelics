using Game.Core;
using Injection;
using Game.Config;
using Game;
using System;
using Game.Level.Area;
using Game.Level.Entity;
using Game.Level.Place;
using Game.Level.Item;

namespace Game.Level.Elevator
{
    public sealed class ElevatorModel : EntityModel
    {
        public int NextHotel;

        public ElevatorModel(string id, int lvl, EntityType type, ElevatorConfig config, int nextHotel, int maxLvl) : base(id, lvl, type)
        {
            TargetPurchaseValue = maxLvl;
            Area = config.Area;
            PricePurchase = config.PricePurchase;
            NextHotel = nextHotel;
        }

        public override int GetLvlLength()
        {
            return 0;
        }

        public override void GetUpdatedValues()
        {

        }
    }

    public sealed class ElevatorController : IDisposable
    {
        private readonly ElevatorModel _model;
        private readonly ElevatorView _view;
        private readonly StateManager<ElevatorState> _stateManager;

        private readonly ItemController _showHudItem;
        private readonly ItemController _buyUpdateItem;

        public ElevatorView View => _view;
        public ElevatorModel Model => _model;

        public ItemController ShowHudItem => _showHudItem;
        public ItemController BuyUpdateItem => _buyUpdateItem;

        public AreaController Area { get; internal set; }

        public ElevatorController(ElevatorView view, Context context)
        {
            _view = view;

            var subContext = new Context(context);
            var injector = new Injector(subContext);

            subContext.Install(this);
            subContext.Install(injector);

            _stateManager = new StateManager<ElevatorState>();
            _stateManager.IsSendLogs = false;

            injector.Inject(_stateManager);

            var gameManager = context.Get<GameManager>();
            var gameConfig = context.Get<GameConfig>();
            var levelView = context.Get<LevelView>();

            int nextHotel = gameManager.Model.Hotel;
            nextHotel++;
            string id = gameManager.Model.GenerateEntityID(gameManager.Model.Hotel, _view.Type, nextHotel);
            _view.name = id;
            int lvl = gameManager.Model.LoadPlaceLvl(id);

            _model = new ElevatorModel(id, lvl, _view.Type, _view.Config, nextHotel, levelView.MaxLevels);
            _model.IsPurchased = gameManager.Model.LoadPlaceIsPurchased(id);
            _model.Cash = gameManager.Model.LoadPlaceCash(_model.ID);

            _view.HudView.Model = _model;

            _showHudItem = new ItemController(_view.ShowHudItemView.transform, gameConfig.ElevatorItemRadius, _view.ShowHudItemView.Type);
            _buyUpdateItem = new ItemController(_view.ItemBuyUpdateView.transform, gameConfig.BuyUpdateRadius, _view.ItemBuyUpdateView.Type);

            SwitchToState(new ElevatorInitializeState());
        }

        public void SwitchToState<T>(T instance) where T : ElevatorState
        {
            _stateManager.SwitchToState(instance);
        }

        public void Dispose()
        {
            _stateManager.Dispose();
        }

        public bool IsPurchasable(int lvl, int targetLvl)
        {
            return lvl >= targetLvl;
        }
    }

}



