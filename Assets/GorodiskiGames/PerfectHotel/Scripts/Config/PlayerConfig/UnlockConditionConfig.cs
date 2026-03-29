using UnityEngine;


namespace Game.Config
{
    public enum UnlockConditionType
    {
        Free,
        HotelPurchase,
        GameLogin,
        WatchAds
    }

    public class UnlockConditionConfig : ScriptableObject
    {
        public UnlockConditionType Type;
    }
}
