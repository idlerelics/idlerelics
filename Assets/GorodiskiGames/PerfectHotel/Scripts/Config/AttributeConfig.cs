using System;
using UnityEngine;

namespace Game.Config
{
    public enum AttributeType
    {
        Capacity,
        WalkSpeed,
        RotateSpeed
    }

    [Serializable]
    public struct AttributeInfo
    {
        public AttributeType Type;
        public float AddValue;
    }

    [Serializable]
    [CreateAssetMenu(menuName = "Config/AttributeConfig")]
    public sealed class AttributeConfig : ScriptableObject
    {
        public string LabelKey;
        public AttributeType Type;
        public Sprite Icon;
        public float NominalValue;
    }
}
