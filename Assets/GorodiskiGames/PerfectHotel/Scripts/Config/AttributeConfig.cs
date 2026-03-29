using System;
using UnityEngine;

namespace Game.Config
{
    /// <summary>
    /// Enum defining the types of player attributes that can be upgraded.
    /// </summary>
    public enum AttributeType
    {
        Capacity,     // How many items the player can carry
        WalkSpeed,    // How fast the player moves
        RotateSpeed   // How fast the player turns
    }

    /// <summary>
    /// A single attribute bonus entry (used in PlayerConfig).
    /// 'struct' is a value type (copied on assignment, unlike classes which are reference types).
    /// Structs are good for small, simple data that doesn't need inheritance.
    ///
    /// [Serializable] allows this struct to be displayed in the Unity Inspector
    /// and saved as part of a ScriptableObject.
    /// </summary>
    [Serializable]
    public struct AttributeInfo
    {
        public AttributeType Type;   // Which attribute this applies to
        public float AddValue;       // How much to add to the base value
    }

    /// <summary>
    /// Configuration asset defining a single attribute (Capacity, WalkSpeed, or RotateSpeed).
    /// Used in the shop/upgrade UI to display attribute information.
    /// Created via Unity menu: Create > Config > AttributeConfig.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(menuName = "Config/AttributeConfig")]
    public sealed class AttributeConfig : ScriptableObject
    {
        public string LabelKey;          // Localization key for the attribute name
        public AttributeType Type;       // Which attribute this config represents
        public Sprite Icon;              // Icon displayed in the UI
        public float NominalValue;       // The base/default value for this attribute
    }
}
