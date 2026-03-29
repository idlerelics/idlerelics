using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    /// <summary>
    /// A clickable UI slot that displays a room visual style option.
    /// Used in the Room Upgrade HUD to let the player pick a wall design.
    ///
    /// Each slot shows a preview icon (sprite) and fires an event with its
    /// index when clicked, so the mediator knows which style was selected.
    ///
    /// "sealed" means no other class can inherit from RoomSlotView.
    /// </summary>
    public sealed class RoomSlotView : BaseHud
    {
        /// <summary>
        /// Event fired when this slot is clicked, passing the slot's index.
        /// The mediator subscribes to this to handle the selection.
        ///
        /// "event Action&lt;int&gt;" is a C# event that passes an int parameter
        /// to all subscribers when invoked.
        /// </summary>
        public event Action<int> ON_SLOT_CLICK;

        // The clickable button component on this slot
        [SerializeField] private Button _button;

        // The Image component that displays the style preview icon
        [SerializeField] private Image _icon;

        // The index of this slot in the list of available visual styles
        private int _index;

        /// <summary>
        /// Sets up this slot with a preview icon and its position index.
        /// Called by the mediator after instantiation.
        /// </summary>
        /// <param name="icon">The sprite to display as a preview of this visual style.</param>
        /// <param name="index">The index identifying this visual style option.</param>
        public void Initialize(Sprite icon, int index)
        {
            _icon.sprite = icon;
            _index = index;
        }

        /// <summary>
        /// Called when this slot's GameObject becomes active.
        /// Subscribes to the button's click event.
        ///
        /// AddListener registers a callback method to Unity's button click event.
        /// </summary>
        protected override void OnEnable()
        {
            _button.onClick.AddListener(OnSlotButtonClick);
        }

        /// <summary>
        /// Called when this slot's GameObject becomes inactive.
        /// Unsubscribes from the button's click event to prevent stale callbacks.
        /// Always pair AddListener with RemoveListener.
        /// </summary>
        protected override void OnDisable()
        {
            _button.onClick.RemoveListener(OnSlotButtonClick);
        }

        /// <summary>
        /// Button click handler. Fires the ON_SLOT_CLICK event with this slot's index.
        /// The "?." (null-conditional operator) ensures we only invoke if there are subscribers.
        /// </summary>
        void OnSlotButtonClick()
        {
            ON_SLOT_CLICK?.Invoke(_index);
        }
    }
}
