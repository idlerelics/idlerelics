using Game.Core;
using Game.Level.Item;
using Game.UI;
using Injection;
using UnityEngine;

namespace Game.Level.Player
{
    /// <summary>
    /// Base state for when the player is interacting with an item (cleaning, reception, purchasing, etc.).
    /// Handles the common logic:
    /// 1. Remove the item from the available items list (so other systems don't also interact with it)
    /// 2. Count down the item's duration each frame
    /// 3. If the joystick is used, interrupt and switch to walk state
    /// 4. When duration reaches zero, fire the "item finished" event
    ///
    /// Subclasses (PlayerCleaningState, PlayerReceptionState, etc.) customize
    /// the animation and what happens when the item finishes.
    /// </summary>
    public class PlayerItemState : PlayerState
    {
        // Dependencies injected by the DI container
        [Inject] protected Timer _timer;          // Provides per-frame TICK events
        [Inject] protected GameView _gameView;    // Access to joystick input and camera
        [Inject] protected GameManager _gameManager; // Central game state manager

        protected ItemController _item; // The item the player is interacting with

        public PlayerItemState(ItemController item)
        {
            _item = item;
        }

        /// <summary>
        /// Removes the item from the available list and starts listening for frame ticks.
        /// </summary>
        public override void Initialize()
        {
            // Remove item so no other player/NPC tries to use it simultaneously
            _gameManager.RemoveItem(_item);

            _timer.TICK += OnTick; // Subscribe to per-frame updates
        }

        /// <summary>
        /// Re-adds the item to the available list if it still has duration remaining
        /// (meaning the player walked away before finishing).
        /// </summary>
        public override void Dispose()
        {
            _timer.TICK -= OnTick;

            // If the item still has time left, put it back so it can be used again later
            if (_item.Model.Duration > 0f)
                _gameManager.AddItem(_item);
        }

        /// <summary>
        /// Called every frame. Checks for joystick input (to interrupt) and processes the item.
        /// </summary>
        private void OnTick()
        {
            // If the player moves the joystick, stop interacting and walk away
            if (_gameView.Joystick.HasInput)
                _player.SwitchToState(new PlayerWalkState());

            PlayerOnItem(); // Process the item interaction (countdown)
        }

        /// <summary>
        /// Decreases the item's remaining duration each frame.
        /// Time.deltaTime is the seconds since the last frame, making this frame-rate independent.
        /// SetChanged() notifies observers that the model has been updated (for UI progress bars, etc.).
        /// </summary>
        public virtual void PlayerOnItem()
        {
            _item.Model.Duration -= Time.deltaTime;
            _item.Model.SetChanged(); // Trigger UI updates (e.g., progress bar)

            if (_item.Model.Duration > 0f) return; // Not done yet, keep going
            OnItemFinished();
        }

        /// <summary>
        /// Called when the item's duration reaches zero.
        /// Fires the finished event and returns the player to idle.
        /// </summary>
        public virtual void OnItemFinished()
        {
            _item.FireItemFinished(); // Notify listeners that the interaction is complete
            _player.SwitchToState(new PlayerIdleState());
        }
    }
}
