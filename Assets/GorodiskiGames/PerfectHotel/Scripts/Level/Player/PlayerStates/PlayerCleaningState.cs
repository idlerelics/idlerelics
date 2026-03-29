using Game.Level.Item;

namespace Game.Level.Player
{
    /// <summary>
    /// State for when the player is cleaning a room.
    /// Inherits from PlayerItemState, which handles the countdown timer.
    /// The only addition is playing the cleaning animation when the state starts.
    /// </summary>
    public sealed class PlayerCleaningState : PlayerItemState
    {
        public PlayerCleaningState(ItemController item) : base(item)
        {
            _item = item;
        }

        /// <summary>
        /// Plays the cleaning animation, then starts the parent's timer logic.
        /// </summary>
        public override void Initialize()
        {
            _player.View.Clean(); // Trigger the cleaning animation on the player model

            base.Initialize(); // Start the item interaction timer from PlayerItemState
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
