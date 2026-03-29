using System;
using UnityEngine;

namespace Game.Config
{
    public class EntityConfig : ScriptableObject
    {
        [Min(0)] public int Number;
        [Min(1)] public int Area;
    }

    [Serializable]
    [CreateAssetMenu(menuName = "config/roomconfig")]
    public sealed class RoomConfig : EntityConfig
    {
        public int PricePurchase;
        [Min(0)] public int TargetPurchaseProgress;
        public int PurchaseProgressReward;
        public int UpdateProgressReward;
        public float StayDuration;
        public RoomLvlConfig[] Lvls;
    }

    [Serializable]
    public sealed class RoomLvlConfig
    {
        public int PriceUpdate;
        public int TargetUpdateProgress;
        public float CleaningTime;
        public int EntranceFee;
        public int StayFee;
    }
}