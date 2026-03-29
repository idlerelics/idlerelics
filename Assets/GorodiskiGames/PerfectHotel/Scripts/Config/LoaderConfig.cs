using System;
using UnityEngine;

namespace Game.Config
{
    /// <summary>
    /// Configuration for a loader (worker NPC that carries items).
    /// Created via Unity menu: Create > config > loaderconfig.
    ///
    /// Loaders transport items between rooms and the utility area.
    /// They can be upgraded to move faster.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(menuName = "config/loaderconfig")]
    public sealed class LoaderConfig : EntityConfig
    {
        public int PricePurchase;                      // Cost to hire this loader
        [Min(0)] public int TargetPurchaseProgress;    // Progress needed before hiring is available
        public LoaderLvlConfig[] Lvls;                 // Stats for each upgrade level
    }

    /// <summary>
    /// Configuration for a single loader upgrade level.
    /// Higher levels mean faster movement speed.
    /// </summary>
    [Serializable]
    public sealed class LoaderLvlConfig
    {
        public int TargetUpdateProgress;  // Progress needed to unlock this upgrade
        public int Price;                 // Cost to upgrade to this level
        public float Speed;               // Movement speed at this level
    }
}
