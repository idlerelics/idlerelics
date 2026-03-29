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
    public sealed class ReceptionModel : EntityModel
    {
        public int ReceptionistCount;
        public float UnitProceedTime = 1f;

        public ReceptionLvlConfig[] Lvls;
        public int ItemsToShow = 1;

        public ReceptionModel(string id, int lvl, EntityType type, ReceptionConfig config) : base(id, lvl, type)
        {
            Lvls = config.Lvls;

            UpdateModel();
        }

        public override int GetLvlLength()
        {
            return Lvls.Length;
        }

        public override void GetUpdatedValues()
        {
            TargetUpdateProgress = Lvls[LvlNext].TargetUpdateProgress;
            PriceUpdate = Lvls[LvlNext].Price;
            ReceptionistCount = Lvls[Lvl].ReceptionistCount;
        }
    }

    public sealed class ReceptionController : EntityController
    {
        private readonly ReceptionModel _model;
        private readonly ReceptionView _view;
        private readonly StateManager<ReceptionState> _stateManager;
        private readonly List<ItemController> _items;
        private readonly ItemController _itemCashPile;
        private readonly ItemController _itemBuyUpdate;
        private readonly LineController _line;

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

            var subContext = new Context(context);
            var injector = new Injector(subContext);

            subContext.Install(this);
            subContext.Install(injector);

            _stateManager = new StateManager<ReceptionState>();
            _stateManager.IsSendLogs = false;

            injector.Inject(_stateManager);

            var gameManager = context.Get<GameManager>();
            var gameConfig = context.Get<GameConfig>();

            string id = gameManager.Model.GenerateEntityID(gameManager.Model.Hotel, _view.Type, 0);
            int lvl = gameManager.Model.LoadPlaceLvl(id);

            _model = new ReceptionModel(id, lvl, _view.Type, _view.Config);
            _model.IsPurchased = true;

            var cash = gameManager.Model.LoadPlaceCash(_model.ID);
            _model.Cash = cash;

            _view.HudView.Model = _model;

            _items = new List<ItemController>();
            foreach (var itemView in _view.Items)
            {
                var item = new ItemReceptionController(itemView.transform, gameConfig.ReceptionItemRadius, itemView.Type, itemView);
                _items.Add(item);
            }

            _itemCashPile = new ItemController(view.ItemCashPileView.transform, gameConfig.CashPileRadius, view.ItemCashPileView.Type);
            _itemBuyUpdate = new ItemController(view.ItemBuyUpdateView.transform, gameConfig.BuyUpdateRadius, view.ItemBuyUpdateView.Type);

            _line = new LineController(_view.Line);

            SwitchToState(new ReceptionIdleState());
        }

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



