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
    /// <summary>
    /// Base class for all staff characters (player and NPCs).
    /// "abstract" means you cannot create an instance of this class directly --
    /// you must create a subclass (like PlayerController) that fills in the details.
    /// It extends EntityUpdatableController, which provides upgrade/purchase logic.
    /// </summary>
    public abstract class StaffController : EntityUpdatableController
    {
        // Events use the "Action" delegate type. Other scripts can subscribe to these
        // events to be notified when something happens (e.g., when staff is purchased).
        // The "?" in ON_PURCHASED?.Invoke() means "only fire if someone is listening."
        public event Action<StaffController> ON_PURCHASED;
        public event Action<StaffController> ON_ARRIVED_HOME;
        public event Action<StaffController> ON_ARRIVED_TO_ITEM;
        public event Action<StaffController> ON_ARRIVED_TO_UTILITY;

        // "abstract" properties and methods MUST be implemented by any subclass.
        // Transform is a Unity type that holds position, rotation, and scale of a GameObject.
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

        /// <summary>Notifies all listeners that this staff member has been purchased.</summary>
        public void FireStaffPurchased()
        {
            ON_PURCHASED?.Invoke(this);
        }

        /// <summary>Notifies listeners that this staff arrived at a utility (e.g., cleaning station).</summary>
        public void FireStaffArrivedToUtility()
        {
            ON_ARRIVED_TO_UTILITY?.Invoke(this);
        }

        /// <summary>Notifies listeners that this staff arrived at an item (e.g., a room item).</summary>
        public void FireArrivedToItem()
        {
            ON_ARRIVED_TO_ITEM?.Invoke(this);
        }

        /// <summary>Notifies listeners that this staff arrived back at their home position.</summary>
        internal void FireArrivedHome()
        {
            ON_ARRIVED_HOME?.Invoke(this);
        }
    }

    /// <summary>
    /// Determines whether a player character is unlocked based on various conditions
    /// (e.g., purchasing a hotel, watching ads, or logging in for a number of days).
    /// "sealed" means no other class can inherit from this one.
    /// </summary>
    public sealed class UnlockConditionModel
    {
        private const string _hotelPurchasePattern = "OPEN {0} HOTEL";

        private const string _watchAdsLeftPattern = "WATCH ADS {0} TIMES\n{1} LEFT";
        private const string _watchAdsPattern = "WATCH ADS {0} TIMES";

        private const string _straightWord = "STRAIGHT";
        private const string _loginDaysLeftPattern = "LOGIN TO THE GAME {0} DAYS {1}\n{2} DAYS LEFT";

        public bool IsUnlocked;  // true if the player character is available to use
        public string Message;   // UI message shown to the player about unlock progress

        /// <summary>
        /// Constructor: runs once when this object is created.
        /// Checks the unlock condition type and determines if the player is unlocked.
        /// </summary>
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

        /// <summary>
        /// Loads a saved target value from PlayerPrefs (Unity's simple key-value save system).
        /// PlayerPrefs stores small amounts of data on the player's device between sessions.
        /// </summary>
        public int LoadTargetValue(int playerIndex, string conditionKey, int defaultValue)
        {
            var key = conditionKey + playerIndex;
            var result = PlayerPrefs.GetInt(key); // Read saved integer from disk
            if (result == 0)
            {
                result = defaultValue;
                PlayerPrefs.SetInt(key, result);
            }
            return result;
        }
    }

    /// <summary>
    /// Holds all the data for a player character: body mesh, speed, capacity, inventories, etc.
    /// Extends "Observable" so other parts of the code can listen for changes to this data
    /// (this is part of the Observer design pattern).
    /// </summary>
    public class PlayerModel : Observable
    {
        public Mesh BodyMesh;       // The 3D mesh used for the player's body
        public UnitSexType Sex;     // Male or female character type
        public string Label;        // Display name of the character
        public int Index;           // Unique index identifying this player
        public Sprite Icon;         // 2D icon image shown in UI
        public float NominalSpeed;  // The base walking speed (before any modifiers)

        public bool IsSelected;

        public readonly UnlockConditionModel UnlockModel;

        // Dictionary = a lookup table. Key: AttributeType (e.g., Speed), Value: the attribute data.
        public readonly Dictionary<AttributeType, AttributeModel> Attributes;
        // List of items the player is currently carrying.
        public readonly List<InventoryController> Inventories;

        /// <summary>
        /// Constructor: builds a PlayerModel from config data (set up in the Unity Editor).
        /// </summary>
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

        /// <summary>Returns true if the player can pick up more items.</summary>
        public bool HasInventorySpace()
        {
            return Inventories.Count < Capacity;
        }

        /// <summary>Returns true if the player is carrying at least one item.</summary>
        public bool HasInventory()
        {
            return Inventories.Count > 0;
        }

        // Properties with "get" are like read-only shortcuts to computed values.
        // Capacity reads the current value from the Capacity attribute.
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

    /// <summary>
    /// The main player controller. This is the "brain" of the player character.
    /// It manages the player's state (idle, walking, etc.) using a State Machine pattern,
    /// holds references to the player's View (visuals) and Model (data), and provides
    /// methods that other systems call to control the player.
    /// "sealed" means no class can inherit from this.
    /// </summary>
    public sealed class PlayerController : StaffController
    {
        private const float _inventoryHeight = 0.6f; // Vertical spacing between stacked inventory items

        public event Action ON_IDLE; // Fired when the player enters the idle state

        private PlayerModel _model;

        private readonly PlayerView _view;
        // StateManager handles switching between different player states (idle, walking, etc.).
        // This is the "State Machine" design pattern -- only one state is active at a time.
        private readonly StateManager<PlayerState> _stateManager;

        public PlayerView View => _view;
        public PlayerModel Model => _model;

        public override Transform InventoryHolder => _view.InventoryHolder;
        public override InventoryType TargetInventory => InventoryType.None;

        public override int Area => -1;

        /// <summary>Calculates where to place an inventory item visually, stacking them vertically.</summary>
        public Vector3 GetInventoryPosition(int index)
        {
            return InventoryHolder.transform.position + new Vector3(0f, index * _inventoryHeight, 0f);
        }

        /// <summary>
        /// Constructor: sets up the player with its view, model, and dependency injection context.
        /// "Context" and "Injector" are part of a Dependency Injection (DI) system --
        /// DI lets objects get their dependencies automatically instead of creating them manually.
        /// </summary>
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

        /// <summary>Cleans up the state manager when this controller is no longer needed.</summary>
        public override void Dispose()
        {
            _stateManager.Dispose();
        }

        /// <summary>
        /// Switches the player to a new state (e.g., from Walking to Idle).
        /// The "where T : PlayerState" constraint means T must be a PlayerState subclass.
        /// </summary>
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

        /// <summary>Fires the ON_IDLE event so other systems know the player is now idle.</summary>
        internal void FireIdle()
        {
            ON_IDLE?.Invoke();
        }

        /// <summary>Updates the player's data model and tells the view to reflect the change.</summary>
        public void SetModel(PlayerModel model)
        {
            _model = model;
            _view.Model = model;
        }
    }
}

