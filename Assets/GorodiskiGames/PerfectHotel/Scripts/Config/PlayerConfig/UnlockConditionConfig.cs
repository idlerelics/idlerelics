using UnityEngine;


namespace Game.Config
{
    /// <summary>
    /// Enum listing the ways a player character can be unlocked.
    /// </summary>
    public enum UnlockConditionType
    {
        Free,            // Available from the start (no requirements)
        HotelPurchase,   // Unlocked by purchasing a specific hotel/dorm level
        GameLogin,       // Unlocked by logging in for a certain number of days
        WatchAds         // Unlocked by watching a certain number of ads
    }

    /// <summary>
    /// Base class for player unlock conditions.
    /// Subclasses (WatchAdsConditionConfig, HotelPurchaseConditionConfig) add
    /// specific requirements like "watch 5 ads" or "buy hotel 2".
    ///
    /// This uses inheritance to handle different condition types:
    /// - Free characters have just the base UnlockConditionConfig (Type = Free)
    /// - Others use specialized subclasses with additional fields
    /// </summary>
    public class UnlockConditionConfig : ScriptableObject
    {
        public UnlockConditionType Type; // What kind of condition this is
    }
}
