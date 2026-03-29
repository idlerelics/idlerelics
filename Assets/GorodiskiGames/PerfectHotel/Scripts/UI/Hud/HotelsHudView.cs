using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    /// <summary>
    /// View component for the Hotels selection HUD screen.
    /// Holds references to the UI elements needed by the HotelsHudMediator:
    /// a close button, a scrollable container for hotel slots, and the slot prefab.
    ///
    /// This is a "thin view" -- it contains no logic, only Inspector references.
    /// All behavior is handled by the corresponding HotelsHudMediator.
    ///
    /// "sealed" means no other class can inherit from HotelsHudView.
    /// </summary>
    public sealed class HotelsHudView : BaseHud
    {
        // UI element references assigned in the Unity Inspector
        [SerializeField] private Button _closeButton;          // Button to close the hotels HUD
        [SerializeField] private RectTransform _container;     // Parent container for instantiated hotel slots
        [SerializeField] private GameObject _hotelSlotPrefab;  // Prefab template for each hotel slot entry

        /// <summary>Button that closes the hotels selection screen.</summary>
        public Button CloseButton => _closeButton;

        /// <summary>The prefab used to create individual hotel slot entries at runtime.</summary>
        public GameObject HotelSlotPrefab => _hotelSlotPrefab;

        /// <summary>The RectTransform container where hotel slot instances are parented.</summary>
        public RectTransform Container => _container;

        /// <summary>
        /// Intentionally empty -- event subscriptions are handled by the mediator.
        /// Required by the abstract BaseHud class.
        /// </summary>
        protected override void OnEnable()
        {
        }

        /// <summary>
        /// Intentionally empty -- event unsubscriptions are handled by the mediator.
        /// Required by the abstract BaseHud class.
        /// </summary>
        protected override void OnDisable()
        {
        }
    }
}
