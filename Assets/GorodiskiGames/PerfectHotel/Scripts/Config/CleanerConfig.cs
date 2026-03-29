using System;
using UnityEngine;

namespace Game.Config
{
    [Serializable]
    [CreateAssetMenu(menuName = "config/cleanerconfig")]
    public sealed class CleanerConfig : EntityConfig
    {
        public int PricePurchase;
        public int TargetPurchaseProgress;
        public CleanerLvlConfig[] Lvls;
    }

    [Serializable]
    public sealed class CleanerLvlConfig
    {
        public int TargetUpdateProgress;
        public int Price;
        public float Speed;
    }
}