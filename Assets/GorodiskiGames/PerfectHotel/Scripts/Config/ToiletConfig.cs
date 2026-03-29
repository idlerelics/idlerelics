using System;
using UnityEngine;

namespace Game.Config
{
    [Serializable]
    [CreateAssetMenu(menuName = "config/toiletconfig")]
    public sealed class ToiletConfig : EntityConfig
    {
        public int PricePurchase;
        public int TargetPurchaseProgress;
        public int PurchaseProgressReward;
        public int StayFee;
        public float StayDuration = 3f;
    }
}