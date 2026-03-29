using System;
using UnityEngine;

namespace Game.Config
{
    /// <summary>
    /// Unlock condition: the player must watch a certain number of ads.
    /// Inherits from UnlockConditionConfig (Type will be WatchAds).
    /// Created via Unity menu: Create > Config > WatchAdsConditionConfig.
    ///
    /// [Min(1)] ensures at least 1 ad must be watched (can't set to 0 in the Inspector).
    /// </summary>
    [Serializable]
    [CreateAssetMenu(menuName = "Config/WatchAdsConditionConfig")]
    public sealed class WatchAdsConditionConfig : UnlockConditionConfig
    {
        [Min(1)] public int WatchAdsTimes; // Number of ads that must be watched to unlock
    }
}
