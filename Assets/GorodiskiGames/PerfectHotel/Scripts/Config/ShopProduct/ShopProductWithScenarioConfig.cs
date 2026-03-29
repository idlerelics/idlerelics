using System;
using UnityEngine;

namespace Game.Config
{
    [Serializable]
    [CreateAssetMenu(menuName = "Config/ShopProductWithScenarioConfig")]
    public class ShopProductWithScenarioConfig : ShopProductConfig
    {
        public int Amount;
        [Header("Scenario(Minutes)")]
        public int[] Scenario;
        [Header("Scenario Debug(Minutes)")]
        public int[] ScenarioDebug;
    }
}

