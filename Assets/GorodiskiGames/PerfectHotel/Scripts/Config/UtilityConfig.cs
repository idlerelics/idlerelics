using System;
using UnityEngine;

namespace Game.Config
{
    /// <summary>
    /// Configuration for a utility room (supply closet where loaders pick up items).
    /// Created via Unity menu: Create > config > utilityconfig.
    ///
    /// Each area can have one utility room. It must be purchased before loaders
    /// in that area can operate.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(menuName = "config/utilityconfig")]
    public sealed class UtilityConfig : ScriptableObject
    {
        [Min(1)] public int Area;                    // Which area this utility room belongs to
        public int TargetPurchaseProgress;           // Progress needed before this can be purchased
    }
}
