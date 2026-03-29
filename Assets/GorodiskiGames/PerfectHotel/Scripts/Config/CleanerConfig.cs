using System;
using UnityEngine;

namespace Game.Config
{
    /// <summary>
    /// Configuration for a cleaner NPC (automatically cleans rooms).
    /// Created via Unity menu: Create > config > cleanerconfig.
    ///
    /// Cleaners walk to dirty rooms and clean them without player input.
    /// They can be upgraded to move faster.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(menuName = "config/cleanerconfig")]
    public sealed class CleanerConfig : EntityConfig
    {
        public int PricePurchase;            // Cost to hire this cleaner
        public int TargetPurchaseProgress;   // Progress needed before hiring is available
        public CleanerLvlConfig[] Lvls;      // Stats for each upgrade level
    }

    /// <summary>
    /// Configuration for a single cleaner upgrade level.
    /// Higher levels mean faster movement.
    /// </summary>
    [Serializable]
    public sealed class CleanerLvlConfig
    {
        public int TargetUpdateProgress;  // Progress needed to unlock this upgrade
        public int Price;                 // Cost to upgrade to this level
        public float Speed;               // Movement speed at this level
    }
}
