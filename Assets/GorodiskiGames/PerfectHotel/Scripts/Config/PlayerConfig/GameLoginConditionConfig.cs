using System;
using UnityEngine;

namespace Game.Config
{
    [Serializable]
    [CreateAssetMenu(menuName = "Config/GameLoginConditionConfig")]
    public sealed class GameLoginConditionConfig : UnlockConditionConfig
    {
        [Min(1)] public int DaysCount;
    }
}

