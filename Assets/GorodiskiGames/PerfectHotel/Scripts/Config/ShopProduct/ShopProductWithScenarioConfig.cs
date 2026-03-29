using System;
using UnityEngine;

namespace Game.Config
{
    /// <summary>
    /// Configuration for a shop product that appears on a timed schedule (scenario).
    /// For example, a rewarded ad that offers coins might appear every 5, 10, then 15 minutes.
    ///
    /// The Scenario array defines the timing in minutes between each appearance.
    /// ScenarioDebug provides shorter intervals for testing during development.
    ///
    /// [Header("text")] adds a bold label in the Unity Inspector to organize fields visually.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(menuName = "Config/ShopProductWithScenarioConfig")]
    public class ShopProductWithScenarioConfig : ShopProductConfig
    {
        public int Amount;                             // How much currency/reward is given
        [Header("Scenario(Minutes)")]
        public int[] Scenario;                         // Time intervals (in minutes) for production builds
        [Header("Scenario Debug(Minutes)")]
        public int[] ScenarioDebug;                    // Shorter time intervals for debug/test builds
    }
}
