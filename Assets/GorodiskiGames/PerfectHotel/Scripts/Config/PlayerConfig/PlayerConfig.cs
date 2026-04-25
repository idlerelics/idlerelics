using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Config
{
    /// <summary>
    /// Enum for the sex/gender of a unit (NPC or player character).
    /// Determines which model/animations to use.
    /// </summary>
    public enum UnitSexType
    {
        Male,
        Female
    }

    /// <summary>
    /// Enum identifying which player character slot this config belongs to.
    /// The game supports multiple unlockable player characters (0 through 4).
    /// The explicit "= 0, = 1" values ensure the enum maps to array indices.
    /// </summary>
    public enum PlayerIndex
    {
        Player0 = 0,
        Player1 = 1,
        Player2 = 2,
        Player3 = 3,
        Player4 = 4,
        Player5 = 5,
        Player6 = 6
    }

    /// <summary>
    /// Configuration for a player character. Each character has different stats,
    /// appearance, and unlock conditions.
    /// Created via Unity menu: Create > Config > PlayerConfig.
    ///
    /// The InfoMap dictionary is built at runtime from the Infos array for fast
    /// attribute lookups by type (e.g., "what's this character's WalkSpeed bonus?").
    /// </summary>
    [Serializable]
    [CreateAssetMenu(menuName = "Config/PlayerConfig")]
    public sealed class PlayerConfig : ScriptableObject
    {
        public PlayerIndex Index;          // Which player slot (Player0 through Player4)
        public UnitSexType Sex;            // Male or Female (affects model/animations)
        public string LabelKey;            // Localization key for the character's name
        public Sprite Icon;                // Character portrait for the UI
        public Mesh Body;                  // 3D mesh for the character model
        public Material BodyMaterial;      // Optional per-character material override (null = keep prefab default)
        public AttributeInfo[] Infos;      // Array of attribute bonuses this character provides
        public UnlockConditionConfig UnlockConditionConfig; // How to unlock this character

        /// <summary>
        /// Dictionary mapping AttributeType to AttributeInfo for fast lookups.
        /// Built at runtime by Init() since Unity can't serialize Dictionaries.
        /// </summary>
        public Dictionary<AttributeType, AttributeInfo> InfoMap;

        /// <summary>
        /// Populates the InfoMap dictionary from the serialized Infos array.
        /// Called after loading the config.
        /// </summary>
        public void Init()
        {
            InfoMap = new Dictionary<AttributeType, AttributeInfo>();
            foreach (var info in Infos)
            {
                InfoMap[info.Type] = info; // Map each attribute type to its info
            }
        }
    }
}
