using Game.Level.Place;
using UnityEngine;

namespace Game.Level.Hotel
{
    /// <summary>
    /// View for a hotel wall that should be hidden when a specific area is purchased.
    /// As the player buys new areas (expanding the hotel), certain walls are removed
    /// to visually open up the space. The _hideOnArea field specifies which area number
    /// triggers this wall's removal.
    ///
    /// For example, the wall between Area 1 and Area 2 would have _hideOnArea = 2,
    /// meaning it disappears when the player purchases Area 2.
    /// </summary>
    public sealed class HotelWallView : MonoBehaviour
    {
        [SerializeField] private int _hideOnArea;              // The area number that, when purchased, hides this wall
        [SerializeField] private ConstructionItemView _walls;  // The construction item containing the wall meshes

        /// <summary>The area number whose purchase triggers hiding this wall.</summary>
        public int HideOnArea => _hideOnArea;

        /// <summary>The construction item view containing the actual wall meshes to show/hide.</summary>
        public ConstructionItemView Walls => _walls;
    }
}
