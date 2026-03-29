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
    public abstract class EntityModel : Observable
    {
        public EntityType Type;
        public string ID;

        public long Cash;

        public int Lvl;
        public int LvlNext;

        public bool IsPurchased { get; internal set; }
        public bool IsMaxed { get; internal set; }
        public bool IsLocked { get; internal set; }

        public int PricePurchase;
        public int PriceUpdate;

        public int Area;
        public int TargetPurchaseValue;
        public int TargetUpdateProgress;

        public int PurchaseProgressReward;
        public int UpdateProgressReward;

        public EntityModel(string id, int lvl, EntityType type)
        {
            ID = id;
            Lvl = lvl;
            Type = type;
        }

        public abstract int GetLvlLength();

        public virtual void UpdateModel()
        {
            LvlNext = Lvl + 1;
            int lvlLength = GetLvlLength();
            if (LvlNext >= lvlLength)
            {
                LvlNext = Lvl;
                IsMaxed = true;
            }
            GetUpdatedValues();
        }
        public abstract void GetUpdatedValues();
    }

    public abstract class EntityController : EntityUpdatableController
    {
        public int CameraAngleSign;
        public abstract Transform Transform { get; }
    }

    public abstract class EntityUpdatableController
    {
        public bool IsUpdatable(bool isMaxed, int currentProgress, int targetUpdateProgress)
        {
            return !isMaxed && currentProgress >= targetUpdateProgress;
        }

        public bool IsPurchasable(bool isAreaPurchased, int currentValue, int targetValue)
        {
            return isAreaPurchased && currentValue >= targetValue;
        }
    }
}

namespace Game.Level.Room
{
    public sealed class RoomModel : EntityModel
    {
        public int VisualIndex;
        public float CleaningTime;
        public int StayFee;
        public float StayDuration;
        public int EntranceFee;

        public RoomLvlConfig[] Lvls;

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

        public override void GetUpdatedValues()
        {
            TargetUpdateProgress = Lvls[LvlNext].TargetUpdateProgress;
            PriceUpdate = Lvls[LvlNext].PriceUpdate;
            CleaningTime = Lvls[Lvl].CleaningTime;
            StayFee = Lvls[Lvl].StayFee;
            EntranceFee = Lvls[Lvl].EntranceFee;
        }

        public override int GetLvlLength()
        {
            return Lvls.Length;
        }
    }



    public sealed class RoomController : EntityController, IDisposable
    {
        private readonly RoomModel _model;
        private readonly RoomView _view;
        private readonly StateManager<RoomState> _stateManager;

        private readonly List<ItemRoomController> _items;

        private readonly ItemController _itemCashPile;
        private readonly ItemController _itemBuyUpdate;

        public RoomView View => _view;
        public RoomModel Model => _model;

        public List<ItemRoomController> Items => _items;

        public ItemController ItemCashPile => _itemCashPile;
        public ItemController ItemBuyUpdate => _itemBuyUpdate;

        public AreaController Area { get; internal set; }
        public bool IsAvailable { get; internal set; }

        public override Transform Transform => _view.transform;

        public RoomController(RoomView view, Context context)
        {
            _view = view;

            CameraAngleSign = _view.CameraAngleSign;

            var subContext = new Context(context);
            var injector = new Injector(subContext);

            subContext.Install(this);
            subContext.Install(injector);

            _stateManager = new StateManager<RoomState>();
            _stateManager.IsSendLogs = false;

            injector.Inject(_stateManager);

            var gameManager = context.Get<GameManager>();
            var gameConfig = context.Get<GameConfig>();

            string id = gameManager.Model.GenerateEntityID(gameManager.Model.Hotel, _view.Type, view.Config.Number);
            _view.name = id;
            int lvl = gameManager.Model.LoadPlaceLvl(id);
            int visualIndex = gameManager.Model.LoadPlaceVisualIndex(id);

            _model = new RoomModel(id, lvl, _view.Type, _view.Config, visualIndex);
            _model.IsPurchased = gameManager.Model.LoadPlaceIsPurchased(id);
            _model.Cash = gameManager.Model.LoadPlaceCash(_model.ID);

            _view.HudView.Model = _model;

            _items = new List<ItemRoomController>();
            foreach (var itemView in _view.Items)
            {
                var item = new ItemRoomController(itemView.transform, gameConfig.RoomItemRadius, itemView.Type, itemView, _view.Config.Area);
                _items.Add(item);
            }

            _itemCashPile = new ItemController(_view.ItemCashPileView.transform, gameConfig.CashPileRadius, _view.ItemCashPileView.Type);
            _itemBuyUpdate = new ItemController(_view.ItemBuyUpdateView.transform, gameConfig.BuyUpdateRadius, _view.ItemBuyUpdateView.Type);

            SwitchToState(new RoomInitializeState());
        }

        public void SwitchToState<T>(T instance) where T : RoomState
        {
            _stateManager.SwitchToState(instance);
        }

        public void Dispose()
        {
            _stateManager.Dispose();
        }

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






