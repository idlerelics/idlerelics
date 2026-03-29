using UnityEngine;

namespace Game.Config
{
    [CreateAssetMenu(menuName = "config/elevatorconfig")]
    public sealed class ElevatorConfig : ScriptableObject
    {
        [Min(1)] public int Area;
        public int PricePurchase;
    }
}
