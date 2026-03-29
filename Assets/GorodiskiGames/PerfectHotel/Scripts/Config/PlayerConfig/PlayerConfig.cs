using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Config
{
    public enum UnitSexType
    {
        Male,
        Female
    }

    public enum PlayerIndex
    {
        Player0 = 0,
        Player1 = 1,
        Player2 = 2,
        Player3 = 3,
        Player4 = 4
    }

    [Serializable]
    [CreateAssetMenu(menuName = "Config/PlayerConfig")]
    public sealed class PlayerConfig : ScriptableObject
    {
        public PlayerIndex Index; 
        public UnitSexType Sex;
        public string LabelKey;
        public Sprite Icon;
        public Mesh Body;
        public AttributeInfo[] Infos;
        public UnlockConditionConfig UnlockConditionConfig;

        public Dictionary<AttributeType, AttributeInfo> InfoMap;

        public void Init()
        {
            InfoMap = new Dictionary<AttributeType, AttributeInfo>();
            foreach (var info in Infos)
            {
                InfoMap[info.Type] = info;
            }
        }
    }
}
