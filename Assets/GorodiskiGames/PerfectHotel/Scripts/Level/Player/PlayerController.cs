using System;
using Core;
using Game.Config;
using Game.Core;
using Injection;
using UnityEngine;
using Utilities;
using System.Collections.Generic;
using Game.UI.Hud;
using Game.Level.Inventory;
using Game.Level.Entity;
using Game.Level.Place;

namespace Game.Level.Player
{
    public abstract class StaffController : EntityUpdatableController
    {
        public event Action<StaffController> ON_PURCHASED;
        public event Action<StaffController> ON_ARRIVED_HOME;
        public event Action<StaffController> ON_ARRIVED_TO_ITEM;
        public event Action<StaffController> ON_ARRIVED_TO_UTILITY;

        public abstract Transform InventoryHolder { get; }
        public abstract InventoryType TargetInventory { get; }
        public abstract int Area { get; }

        public int Inventories;

        public abstract void InitializeStaff();

        public abstract void Dispose();

        public abstract void WalkToItem(Vector3 position);

        public abstract void WalkHome();

        public abstract void ArrivedToUtility();

        public abstract void Idle();

        public abstract void ReadyToPurchase();

        public abstract void WalkToUtility(Vector3 position);

        public abstract void Hidden();

        public void FireStaffPurchased()
        {
            ON_PURCHASED?.Invoke(this);
        }

        public void FireStaffArrivedToUtility()
        {
            ON_ARRIVED_TO_UTILITY?.Invoke(this);
        }

        public void FireArrivedToItem()
        {
            ON_ARRIVED_TO_ITEM?.Invoke(this);
        }

        internal void FireArrivedHome()
        {
            ON_ARRIVED_HOME?.Invoke(this);
        }
    }

    public sealed class UnlockConditionModel
    {
        private const string _hotelPurchasePattern = "OPEN {0} HOTEL";

        private const string _watchAdsLeftPattern = "WATCH ADS {0} TIMES\n{1} LEFT";
        private const string _watchAdsPattern = "WATCH ADS {0} TIMES";

        private const string _straightWord = "STRAIGHT";
        private const string _loginDaysLeftPattern = "LOGIN TO THE GAME {0} DAYS {1}\n{2} DAYS LEFT";

        public bool IsUnlocked;
        public string Message;

        public UnlockConditionModel(int playerIndex, UnlockConditionConfig config, GameConfig gameConfig, GameManager gameManager)
        {
            var messageResult = "";
            var isUnlockedResult = false;
            var type = config.Type;

            if (type == UnlockConditionType.Free)
                isUnlockedResult = true;
            else if (type == UnlockConditionType.HotelPurchase)
            {
                var conditionConfig = config as HotelPurchaseConditionConfig;

                var targetHotelIndex = conditionConfig.HotelIndex;
                var elevatorHotel = targetHotelIndex - 1;

                string id = gameManager.Model.GenerateEntityID(elevatorHotel, EntityType.Elevator, targetHotelIndex);
                var isPurchased = gameManager.Model.LoadPlaceIsPurchased(id);
                var hotelConfig = gameConfig.HotelConfigMap[targetHotelIndex];
                var hotelLabel = ColorUtil.ColorString(hotelConfig.Label, Color.green);

                messageResult = string.Format(_hotelPurchasePattern, hotelLabel);
                isUnlockedResult = isPurchased;
            }
            else if (type == UnlockConditionType.WatchAds)
            {
                var conditionConfig = config as WatchAdsConditionConfig;
                var watchAdsTimes = conditionConfig.WatchAdsTimes;
                var targetTimes = LoadTargetValue(playerIndex, GameConstants.kTargetWatchAdsTimes, watchAdsTimes);
                var times = gameManager.Model.LoadWatchAdsTimes();
                var timesLeft = targetTimes - times;

                Color targetTimesLabelColor;
                ColorUtility.TryParseHtmlString("#00A8FF", out targetTimesLabelColor);
                var targetTimesLabel = ColorUtil.ColorString(targetTimes.ToString(), targetTimesLabelColor);
                var timesLeftLabel = ColorUtil.ColorString(timesLeft.ToString(), Color.green);

                isUnlockedResult = times >= targetTimes;
                messageResult = string.Format(_watchAdsLeftPattern, targetTimesLabel, timesLeftLabel);
                if (targetTimes == timesLeft)
                    messageResult = string.Format(_watchAdsPattern, targetTimesLabel);
            }

            else if (type == UnlockConditionType.GameLogin)
            {
                var conditionConfig = config as GameLoginConditionConfig;
                var daysCount = conditionConfig.DaysCount;
                var targetDays = LoadTargetValue(playerIndex, GameConstants.kTargetLoginDays, daysCount);
                var days = gameManager.Model.LoadLoginDays();
                var daysLeft = targetDays - days;

                Color targetDaysLabelColor;
                ColorUtility.TryParseHtmlString("#00A8FF", out targetDaysLabelColor);
                var targetDaysLabel = ColorUtil.ColorString(targetDays.ToString(), targetDaysLabelColor);
                var daysLeftLabel = ColorUtil.ColorString(daysLeft.ToString(), Color.green);

                isUnlockedResult = days >= targetDays;
                var straightWord = ColorUtil.ColorString(_straightWord, Color.red);
                messageResult = string.Format(_loginDaysLeftPattern, targetDaysLabel, straightWord, daysLeftLabel);
            }

            Message = messageResult;
            IsUnlocked = isUnlockedResult;
        }

        public int LoadTargetValue(int playerIndex, string conditionKey, int defaultValue)
        {
            var key = conditionKey + playerIndex;
            var result = PlayerPrefs.GetInt(key);
            if (result == 0)
            {
                result = defaultValue;
                PlayerPrefs.SetInt(key, result);
            }
            return result;
        }
    }

    public class PlayerModel : Observable
    {
        public Mesh BodyMesh;
        public UnitSexType Sex;
        public string Label;
        public int Index;
        public Sprite Icon;
        public float NominalSpeed;

        public bool IsSelected;

        public readonly UnlockConditionModel UnlockModel;

        public readonly Dictionary<AttributeType, AttributeModel> Attributes;
        public readonly List<InventoryController> Inventories;

        public PlayerModel(PlayerConfig config, GameConfig gameConfig, GameManager gameManager)
        {
            Index = (int)config.Index;
            BodyMesh = config.Body;
            Label = config.LabelKey;
            Icon = config.Icon;
            Sex = config.Sex;

            Inventories = new List<InventoryController>();
            Attributes = new Dictionary<AttributeType, AttributeModel>();
            UnlockModel = new UnlockConditionModel(Index, config.UnlockConditionConfig, gameConfig, gameManager);

            foreach (var type in gameConfig.AttributesMap.Keys)
            {
                var attributeConfig = gameConfig.AttributesMap[type];
                var addValue = 0f;

                if (config.InfoMap.ContainsKey(type))
                {
                    var info = config.InfoMap[type];
                    addValue = info.AddValue;
                }

                var model = new AttributeModel(attributeConfig, addValue);
                Attributes[type] = model;
            }

            NominalSpeed = WalkSpeed;
        }

        public bool HasInventorySpace()
        {
            return Inventories.Count < Capacity;
        }

        public bool HasInventory()
        {
            return Inventories.Count > 0;
        }

        public int Capacity
        {
            get { return (int)Attributes[AttributeType.Capacity].Value; }
        }

        public float RotateSpeed
        {
            get { return Attributes[AttributeType.RotateSpeed].Value; }
        }

        public float WalkSpeed
        {
            get { return Attributes[AttributeType.WalkSpeed].Value; }
            set { Attributes[AttributeType.WalkSpeed].Value = value; }
        }
    }

    public sealed class PlayerController : StaffController
    {
        private const float _inventoryHeight = 0.6f;

        public event Action ON_IDLE;

        private PlayerModel _model;

        private readonly PlayerView _view;
        private readonly StateManager<PlayerState> _stateManager;

        public PlayerView View => _view;
        public PlayerModel Model => _model;

        public override Transform InventoryHolder => _view.InventoryHolder;
        public override InventoryType TargetInventory => InventoryType.None;

        public override int Area => -1;

        public Vector3 GetInventoryPosition(int index)
        {
            return InventoryHolder.transform.position + new Vector3(0f, index * _inventoryHeight, 0f);
        }

        public PlayerController(PlayerView view, PlayerModel model, Context context)
        {
            _view = view;

            SetModel(model);

            var subContext = new Context(context);
            var injector = new Injector(subContext);

            subContext.Install(this);
            subContext.InstallByType(this, typeof(PlayerController));
            subContext.Install(injector);

            _stateManager = new StateManager<PlayerState>();
            _stateManager.IsSendLogs = false;

            injector.Inject(_stateManager);
        }

        public override void Dispose()
        {
            _stateManager.Dispose();
        }

        public void SwitchToState<T>(T instance) where T : PlayerState
        {
            _stateManager.SwitchToState(instance);
            _view.CurrentState = instance.GetType();
        }

        public override void WalkToItem(Vector3 position)
        {
        }

        public override void WalkHome()
        {
        }

        public override void ArrivedToUtility()
        {
        }

        public override void Idle()
        {
        }

        public override void WalkToUtility(Vector3 position)
        {
        }

        public override void InitializeStaff()
        {
        }

        public override void Hidden()
        {
        }

        public override void ReadyToPurchase()
        {
        }

        internal void FireIdle()
        {
            ON_IDLE?.Invoke();
        }

        public void SetModel(PlayerModel model)
        {
            _model = model;
            _view.Model = model;
        }
    }
}

