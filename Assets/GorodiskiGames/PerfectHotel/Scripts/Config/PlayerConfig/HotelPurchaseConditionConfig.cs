using System;
using UnityEngine;

namespace Game.Config
{
    [Serializable]
    [CreateAssetMenu(menuName = "Config/HotelPurchaseConditionConfig")]
    public sealed class HotelPurchaseConditionConfig : UnlockConditionConfig
    {
        [Min(1)] public int HotelIndex;
    }
}

