using System;
using UnityEngine;

namespace Game.Config
{
    [Serializable]
    [CreateAssetMenu(menuName = "config/loaderconfig")]
    public sealed class LoaderConfig : EntityConfig
    {
        public int PricePurchase;
        [Min(0)] public int TargetPurchaseProgress;
        public LoaderLvlConfig[] Lvls;
    }

    [Serializable]
    public sealed class LoaderLvlConfig
    {
        public int TargetUpdateProgress;
        public int Price;
        public float Speed;
    }
}