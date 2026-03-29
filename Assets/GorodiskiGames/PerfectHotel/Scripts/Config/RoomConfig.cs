using System;
using UnityEngine;

namespace Game.Config
{
    /// <summary>
    /// Base class for entity configuration assets (rooms, loaders, cleaners).
    /// Each entity belongs to a numbered slot and an area of the level.
    ///
    /// ScriptableObject is a Unity class for storing data as asset files.
    /// Unlike MonoBehaviour, it doesn't need a GameObject -- it lives in your Project folder.
    /// </summary>
    public class EntityConfig : ScriptableObject
    {
        [Min(0)] public int Number;  // This entity's slot number (0-based)
        [Min(1)] public int Area;    // Which area of the level this entity is in
    }

    /// <summary>
    /// Configuration for a room entity. Created via Unity menu: Create > config > roomconfig.
    ///
    /// [Serializable] tells Unity this class can be saved to disk and shown in the Inspector.
    /// [CreateAssetMenu] adds a right-click menu item to create instances of this asset.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(menuName = "config/roomconfig")]
    public sealed class RoomConfig : EntityConfig
    {
        public int PricePurchase;                     // Cost to buy/unlock this room
        [Min(0)] public int TargetPurchaseProgress;   // Progress needed before this room can be purchased
        public int PurchaseProgressReward;            // Progress earned when purchasing this room
        public int UpdateProgressReward;              // Progress earned when upgrading this room
        public float StayDuration;                    // How long a guest stays in this room (seconds)
        public RoomLvlConfig[] Lvls;                  // Configuration for each upgrade level
    }

    /// <summary>
    /// Configuration for a single room upgrade level.
    /// Each room can be upgraded multiple times, and each level has different stats.
    /// </summary>
    [Serializable]
    public sealed class RoomLvlConfig
    {
        public int PriceUpdate;             // Cost to upgrade to this level
        public int TargetUpdateProgress;    // Progress needed to unlock this upgrade
        public float CleaningTime;          // How long it takes to clean at this level (seconds)
        public int EntranceFee;             // One-time fee paid when a guest checks in
        public int StayFee;                 // Fee earned while the guest stays
    }
}
