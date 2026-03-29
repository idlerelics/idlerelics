using System;
using UnityEngine;

namespace Game.Config
{
    public enum ShopProductReward
    {
        NoAds,
        Cash
    }

    [Serializable]
    [CreateAssetMenu(menuName = "Config/ShopProductConfig")]
    public class ShopProductConfig : ScriptableObject
    {
        public string ID;
        public string Title;
        public ShopProductReward Reward;
    }
}