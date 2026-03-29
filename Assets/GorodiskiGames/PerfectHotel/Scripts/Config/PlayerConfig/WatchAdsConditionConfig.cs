using System;
using UnityEngine;

namespace Game.Config
{
    [Serializable]
    [CreateAssetMenu(menuName = "Config/WatchAdsConditionConfig")]
    public sealed class WatchAdsConditionConfig : UnlockConditionConfig
    {
        [Min(1)] public int WatchAdsTimes;
    }
}

