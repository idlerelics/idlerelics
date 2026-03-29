using UnityEngine;

namespace Game.Level.Place
{
    /// <summary>
    /// View for interior construction items that can display different visual icons
    /// depending on the entity's current level or visual index. Extends ConstructionItemView
    /// to add an array of icon sprites.
    ///
    /// For example, a room's interior decoration might change as the player upgrades it,
    /// with each upgrade showing a different icon from this array.
    /// </summary>
    public class ConstructionInsideView : ConstructionItemView
    {
        [SerializeField] private Sprite[] _icons; // Array of icon sprites, indexed by level or visual variant

        /// <summary>Array of sprites representing different visual states of this construction item.</summary>
        public Sprite[] Icons => _icons;
    }

}
