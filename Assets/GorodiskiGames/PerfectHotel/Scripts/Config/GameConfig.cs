// 'using' statements import code libraries so we can use their classes and features.
using System;
using System.Collections.Generic;
using Game.Managers;
using UnityEngine;

// A 'namespace' groups related classes together, like folders for your code.
namespace Game.Config
{
    // [CreateAssetMenu] is an "attribute" -- a tag that adds extra behavior to a class.
    // This one adds a menu item in Unity: right-click in Project > Create > config > gameconfig.
    // It lets you create instances of this ScriptableObject as asset files in your project.
    [CreateAssetMenu(menuName = "config/gameconfig")]

    // 'sealed' means no other class can inherit from this one.
    // 'ScriptableObject' is a special Unity base class for storing shared data as an asset file.
    // Unlike MonoBehaviour, a ScriptableObject does NOT need to be attached to a GameObject.
    // It is great for configuration data that many scripts need to read.
    public sealed class GameConfig : ScriptableObject
    {
        /// <summary>
        /// Loads the GameConfig asset from the "Resources" folder.
        /// 'static' means you can call this method without creating an instance: GameConfig.Load().
        /// Resources.Load looks for an asset named "GameConfig" inside any folder called "Resources".
        /// </summary>
        public static GameConfig Load()
        {
            var result = Resources.Load<GameConfig>("GameConfig");
            result.Init(); // Populate the dictionaries after loading
            return result;
        }

        // [Min(0)] is an attribute that clamps the value in the Unity Inspector so it can't go below 0.
        // 'public' fields are automatically shown in the Unity Inspector for ScriptableObjects.
        [Min(0)] public int DefaultCash;       // The amount of cash the player starts with
        [Min(1)] public int DefaultHotel;      // The starting hotel scene index (must be at least 1)

        // These 'float' fields control how close the player must be to interact with various objects.
        // A 'float' is a decimal number (e.g. 1.0, 0.75, 0.5).
        public float CashPileRadius = 1f;          // Pickup radius for cash piles
        public float ReceptionItemRadius = 0.75f;  // Interaction radius for reception desk items
        public float BuyUpdateRadius = 1f;          // Radius for buy/upgrade interaction zones
        public float RoomItemRadius = 0.75f;        // Interaction radius for room items
        public float ToiletItemRadius = 0.5f;       // Interaction radius for toilet items
        public float ToiletPaperFlyTime = 0.3f;     // How long (in seconds) the toilet paper animation takes
        [Min(1)] public int ToiletVisitsCountMax;    // Max number of times a toilet can be used before cleaning
        public float UtilityItemRadius = 0.5f;       // Interaction radius for utility items
        [Min(1)] public int PlayerInventoriesMax = 3; // Max items the player can carry at once
        public float ElevatorItemRadius = 1f;        // Interaction radius for elevator
        public float EntityRadius = 3f;              // General detection radius for game entities

        // [Header("Units")] adds a bold label/section header in the Unity Inspector.
        // It helps organize fields visually -- purely cosmetic, no effect on code.
        [Header("Units")]
        public float CustomerRotationSpeed = 10f; // How fast customers rotate (turn) in degrees per second
        public float CustomerWalkSpeed = 3f;      // How fast customers walk (units per second)

        // 'readonly' means these fields can only be set in the constructor (below) and never changed after.
        // 'Dictionary<Key, Value>' is a collection that maps keys to values, like a real dictionary.
        // For example, HotelConfigMap maps a hotel's scene index (int) to its HotelConfig data.
        public readonly Dictionary<int, HotelConfig> HotelConfigMap;
        public readonly Dictionary<AttributeType, AttributeConfig> AttributesMap;
        public readonly Dictionary<int, PlayerConfig> PlayersMap;
        public readonly Dictionary<string, ShopProductIAPConfig> ShopProductIAPMap;

        /// <summary>
        /// Constructor: runs once when a new GameConfig instance is created.
        /// Initializes the dictionaries as empty so they can be filled later in Init().
        /// </summary>
        public GameConfig()
        {
            HotelConfigMap = new Dictionary<int, HotelConfig>();
            AttributesMap = new Dictionary<AttributeType, AttributeConfig>();
            PlayersMap = new Dictionary<int, PlayerConfig>();
            ShopProductIAPMap = new Dictionary<string, ShopProductIAPConfig>();
        }

        /// <summary>
        /// Populates the dictionaries from the serialized arrays set in the Inspector.
        /// 'private' means only this class can call this method.
        /// 'foreach' loops through every item in a collection one at a time.
        /// </summary>
        private void Init()
        {
            // Fill HotelConfigMap: each hotel's SceneIndex becomes the key
            foreach (var hotel in _hotels)
            {
                HotelConfigMap[hotel.SceneIndex] = hotel;
            }
            // Fill AttributesMap: each attribute's Type enum becomes the key
            foreach (var attribute in _attributeConfigs)
            {
                AttributesMap[attribute.Type] = attribute;
            }
            // Fill PlayersMap: each player config's Index becomes the key
            foreach (var config in _playerConfigs)
            {
                var index = (int)config.Index; // Cast the enum to an int for use as a dictionary key
                config.Init();
                PlayersMap[index] = config;
            }
            // Fill ShopProductIAPMap: each product's ID string becomes the key
            foreach (var product in _shopProductIAPConfigs)
            {
                ShopProductIAPMap[product.ID] = product;
            }
        }

        [Header("Splash Screen")]
        public float SplashScreenDurationMobile = 1.5f;  // How long the splash screen shows on mobile (seconds)
        public float SplashScreenDurationEditor = 0.1f;   // Shorter splash in the Unity Editor for faster testing

        [Header("Players")]
        // [SerializeField] is an attribute that tells Unity to save and show a 'private' field
        // in the Inspector. Normally, only 'public' fields appear there.
        // This lets you edit the field in the Inspector while keeping it private in code (good practice).
        [SerializeField] private PlayerConfig[] _playerConfigs; // Array of all player configurations

        [Header("Attributes")]
        [SerializeField] private AttributeConfig[] _attributeConfigs; // Array of all attribute configurations

        [Header("Hotels")]
        [SerializeField] private HotelConfig[] _hotels; // Array of all hotel configurations

        [Header("Ads")]
        public AdsProviderType AdsProviderEditor;  // Which ad provider to use in the Unity Editor
        public AdsProviderType AdsProviderMobile;  // Which ad provider to use on mobile devices

        [Header("Shop")]
        [Min(1)] public int AllScenariosDurationHrs;      // Duration of all scenarios in hours
        [Min(1)] public int NoScenarioDurationMinutes;     // Duration when no scenario is active, in minutes
        [SerializeField] private ShopProductIAPConfig[] _shopProductIAPConfigs; // In-app purchase product configs

        // -----------------------------------------------------------------
        // DEBUG / TESTING
        // -----------------------------------------------------------------
        [Header("Debug")]
        [Tooltip(
            "TESTING ONLY: force the game to load a specific hotel scene index at startup, " +
            "ignoring the save. Leave at 0 to use the save normally. " +
            "Does NOT modify the save -- toggling this off returns you to your real progress. " +
            "Must match a valid scene build index (Hotel1 = 1, Hotel2 = 2, ...).")]
        [Min(0)] public int StartHotelOverride = 0;
    }

    // [Serializable] tells Unity this class can be saved/loaded and shown in the Inspector.
    // Without it, Unity would not be able to display HotelConfig fields in the Inspector.
    [Serializable]
    public sealed class HotelConfig
    {
        [Min(1)] public int SceneIndex; // The Unity scene build index for this hotel
        public Sprite Icon;             // The icon image shown in the UI for this hotel
        public string Label;            // The display name of this hotel
    }
}