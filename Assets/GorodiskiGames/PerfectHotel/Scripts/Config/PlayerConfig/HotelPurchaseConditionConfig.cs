using System;
using UnityEngine;

namespace Game.Config
{
    /// <summary>
    /// Unlock condition: the player must have purchased a specific hotel/dorm level.
    /// Inherits from UnlockConditionConfig (Type will be HotelPurchase).
    /// Created via Unity menu: Create > Config > HotelPurchaseConditionConfig.
    ///
    /// For example, HotelIndex = 2 means "buy hotel/dorm 2 to unlock this character."
    /// </summary>
    [Serializable]
    [CreateAssetMenu(menuName = "Config/HotelPurchaseConditionConfig")]
    public sealed class HotelPurchaseConditionConfig : UnlockConditionConfig
    {
        [Min(1)] public int HotelIndex; // Which hotel must be purchased (1-based)
    }
}
