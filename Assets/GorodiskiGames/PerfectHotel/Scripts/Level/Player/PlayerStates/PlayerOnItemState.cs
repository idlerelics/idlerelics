using Game.Level.Item;

namespace Game.Level.Player
{
    /// <summary>
    /// State for when the player is standing on a "BuyUpdate" item (e.g., purchasing an upgrade).
    /// Inherits from PlayerItemState which handles the countdown timer and joystick interruption.
    ///
    /// The ": base(item)" in the constructor calls the parent class constructor,
    /// passing the item up to PlayerItemState.
    /// </summary>
    public sealed class PlayerOnItemState : PlayerItemState
    {
        public PlayerOnItemState(ItemController item) : base(item)
        {
            _item = item;
        }

        /// <summary>
        /// Sets the player to idle animation before calling the parent Initialize(),
        /// which starts the item interaction timer.
        /// </summary>
        public override void Initialize()
        {
            _player.View.Idle(_player.Model.Sex, _gameManager.Model.InventoryTypes.Count);

            base.Initialize();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        /// <summary>
        /// Called each frame while the player is on the item.
        /// Fires the PLAYER_ON_ITEM event so the item knows the player is interacting with it.
        /// </summary>
        public override void PlayerOnItem()
        {
            _item.FirePlayerOnItem();
        }

        /// <summary>
        /// Called when the item's duration reaches zero.
        /// Empty override -- for BuyUpdate items, finishing is handled by the item itself.
        /// </summary>
        public override void OnItemFinished()
        {
        }
    }
}
