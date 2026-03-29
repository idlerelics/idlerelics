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
    public sealed class ToiletModel : EntityModel
    {
        public int VisualIndex;
        public float StayDuration;
        public int StayFee;

        public ToiletModel(string id, int lvl, EntityType type, ToiletConfig config, int visualIndex) : base(id, lvl, type)
        {
            VisualIndex = visualIndex;
            PricePurchase = config.PricePurchase;
            StayFee = config.StayFee;
            StayDuration = config.StayDuration;
            TargetPurchaseValue = config.TargetPurchaseProgress;
            PurchaseProgressReward = config.PurchaseProgressReward;
            Area = config.Area;

            UpdateModel();
        }

        public override int GetLvlLength()
        {
            return 0;
        }

        public override void GetUpdatedValues()
        {

        }
    }

    public sealed class ToiletController : EntityController, IDisposable
    {
        private readonly ToiletModel _model;
        private readonly ToiletView _view;
        private readonly StateManager<ToiletState> _stateManager;
        private readonly Dictionary<ItemToiletController, bool> _cabinesMap;
        private readonly ItemController _itemBuyUpdate;
        private readonly ItemController _itemCashPile;
        private readonly LineController _line;

        public ToiletModel Model => _model;
        public ToiletView View => _view;
        public ItemController ItemBuyUpdate => _itemBuyUpdate;
        public ItemController ItemCashPile => _itemCashPile;
        public Dictionary<ItemToiletController, bool> CabinesMap => _cabinesMap;
        public LineController Line => _line;

        public override Transform Transform => _view.transform;

        public ToiletController(ToiletView view, Context context)
        {
            _view = view;

            CameraAngleSign = _view.CameraAngleSign;

            var subContext = new Context(context);
            var injector = new Injector(subContext);

            subContext.Install(this);
            subContext.InstallByType(this, typeof(ToiletController));
            subContext.Install(injector);

            _stateManager = new StateManager<ToiletState>();
            _stateManager.IsSendLogs = false;

            injector.Inject(_stateManager);

            var gameManager = context.Get<GameManager>();
            var gameConfig = context.Get<GameConfig>();

            string id = gameManager.Model.GenerateEntityID(gameManager.Model.Hotel, _view.Type, view.Config.Number);
            _view.name = id;
            int lvl = gameManager.Model.LoadPlaceLvl(id);
            int visualIndex = gameManager.Model.LoadPlaceVisualIndex(id);

            _model = new ToiletModel(id, lvl, _view.Type, _view.Config, visualIndex);
            _model.IsPurchased = gameManager.Model.LoadPlaceIsPurchased(id);
            _model.Cash = gameManager.Model.LoadPlaceCash(_model.ID);

            _view.HudView.Model = _model;

            _cabinesMap = new Dictionary<ItemToiletController, bool>();
            int cabineNumber = 0;
            foreach (var itemView in _view.Items)
            {
                string cabineID = _model.ID + itemView.Type + cabineNumber;
                var cabine = new ItemToiletController(itemView.transform, gameConfig.ToiletItemRadius, itemView.Type, itemView, cabineID, _model.Area);

                cabine.StayDuration = _model.StayDuration;
                cabine.VisitsCountMax = gameConfig.ToiletVisitsCountMax;
                cabine.VisitsCount = gameManager.Model.LoadVisitsCount(cabine.ID);

                _cabinesMap.Add(cabine, cabine.IsAvailable);

                cabineNumber++;
            }

            _itemCashPile = new ItemController(view.ItemCashPileView.transform, gameConfig.CashPileRadius, view.ItemCashPileView.Type);
            _itemBuyUpdate = new ItemController(view.ItemBuyUpdateView.transform, gameConfig.BuyUpdateRadius, view.ItemBuyUpdateView.Type);

            _line = new LineController(_view.Line);

            SwitchToState(new ToiletInitializeState());
        }

        public void SwitchToState<T>(T instance) where T : ToiletState
        {
            _stateManager.SwitchToState(instance);
        }

        public void Dispose()
        {
            _stateManager.Dispose();
        }

        public int GetTotalReward()
        {
            return _model.PurchaseProgressReward;
        }

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

