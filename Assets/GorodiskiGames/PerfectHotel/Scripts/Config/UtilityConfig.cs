using System;
using UnityEngine;

namespace Game.Config
{
    [Serializable]
    [CreateAssetMenu(menuName = "config/utilityconfig")]
    public sealed class UtilityConfig : ScriptableObject
    {
        [Min(1)] public int Area;
        public int TargetPurchaseProgress;
    }
}