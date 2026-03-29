using System;
using System.Collections.Generic;
using Game.Managers;
using UnityEngine;

namespace Game.Config
{
    [CreateAssetMenu(menuName = "config/gameconfig")]
    public sealed class GameConfig : ScriptableObject
    {
        public static GameConfig Load()
        {
            var result = Resources.Load<GameConfig>("GameConfig");
            result.Init();
            return result;
        }

        [Min(0)] public int DefaultCash;
        [Min(1)] public int DefaultHotel;

        public float CashPileRadius = 1f;
        public float ReceptionItemRadius = 0.75f;
        public float BuyUpdateRadius = 1f;
        public float RoomItemRadius = 0.75f;
        public float ToiletItemRadius = 0.5f;
        public float ToiletPaperFlyTime = 0.3f;
        [Min(1)] public int ToiletVisitsCountMax;
        public float UtilityItemRadius = 0.5f;
        [Min(1)] public int PlayerInventoriesMax = 3;
        public float ElevatorItemRadius = 1f;
        public float EntityRadius = 3f;

        [Header("Units")]
        public float CustomerRotationSpeed = 10f;
        public float CustomerWalkSpeed = 3f;

        public readonly Dictionary<int, HotelConfig> HotelConfigMap;
        public readonly Dictionary<AttributeType, AttributeConfig> AttributesMap;
        public readonly Dictionary<int, PlayerConfig> PlayersMap;
        public readonly Dictionary<string, ShopProductIAPConfig> ShopProductIAPMap;

        public GameConfig()
        {
            HotelConfigMap = new Dictionary<int, HotelConfig>();
            AttributesMap = new Dictionary<AttributeType, AttributeConfig>();
            PlayersMap = new Dictionary<int, PlayerConfig>();
            ShopProductIAPMap = new Dictionary<string, ShopProductIAPConfig>();
        }

        private void Init()
        {
            foreach (var hotel in _hotels)
            {
                HotelConfigMap[hotel.SceneIndex] = hotel;
            }
            foreach (var attribute in _attributeConfigs)
            {
                AttributesMap[attribute.Type] = attribute;
            }
            foreach (var config in _playerConfigs)
            {
                var index = (int)config.Index;
                config.Init();
                PlayersMap[index] = config;
            }
            foreach (var product in _shopProductIAPConfigs)
            {
                ShopProductIAPMap[product.ID] = product;
            }
        }

        [Header("Splash Screen")]
        public float SplashScreenDurationMobile = 1.5f;
        public float SplashScreenDurationEditor = 0.1f;

        [Header("Players")]
        [SerializeField] private PlayerConfig[] _playerConfigs;

        [Header("Attributes")]
        [SerializeField] private AttributeConfig[] _attributeConfigs;

        [Header("Hotels")]
        [SerializeField] private HotelConfig[] _hotels;

        [Header("Ads")]
        public AdsProviderType AdsProviderEditor;
        public AdsProviderType AdsProviderMobile;

        [Header("Shop")]
        [Min(1)] public int AllScenariosDurationHrs;
        [Min(1)] public int NoScenarioDurationMinutes;
        [SerializeField] private ShopProductIAPConfig[] _shopProductIAPConfigs;
    }

    [Serializable]
    public sealed class HotelConfig
    {
        [Min(1)] public int SceneIndex;
        public Sprite Icon;
        public string Label;
    }
}