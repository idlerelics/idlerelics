using Game.Config;
using Game.Level.Place;
using UnityEngine;

namespace Game.Level.Room
{
    /// <summary>
    /// The visual representation of a hotel room in the scene.
    /// Inherits from PlaceWithItemsReusableView, which provides shared functionality
    /// for places that contain interactive items (like beds, desks).
    ///
    /// [SerializeField] fields are set in the Unity Inspector by dragging references
    /// from the scene onto them. This lets designers configure rooms without code.
    /// </summary>
    public sealed class RoomView : PlaceWithItemsReusableView
    {
        [SerializeField] private ConstructionInsideView _insideWalls;  // The interior walls visual
        [SerializeField] private RoomConfig _config;                    // Config asset with room settings
        [SerializeField] private GameObject _lightDark;                 // Dark overlay shown when room is unavailable
        [SerializeField] private Transform _customerPosition;           // Where the customer stands inside the room

        public ConstructionInsideView InsideWalls => _insideWalls;
        public RoomConfig Config => _config;
        public Transform CustomerPosition => _customerPosition;

        /// <summary>Turns the dark light overlay on or off. SetActive(true) shows it, false hides it.</summary>
        internal void SetDarkLight(bool value)
        {
            _lightDark.SetActive(value);
        }

        /// <summary>
        /// Awake() is called by Unity when this GameObject is first created.
        /// "base.Awake()" calls the parent class's Awake first, then we hide the dark overlay.
        /// "override" means we're replacing the parent's version of this method.
        /// </summary>
        public override void Awake()
        {
            base.Awake();
            _lightDark.SetActive(false); // Start with the room lit (not darkened)
        }
    }
}

