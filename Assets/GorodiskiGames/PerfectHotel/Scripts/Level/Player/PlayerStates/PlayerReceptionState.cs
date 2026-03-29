using Game.Level.Item;

namespace Game.Level.Player
{
    /// <summary>
    /// State for when the player is at the reception desk.
    /// Inherits from PlayerItemState, which handles the countdown timer
    /// (checking guests in) and joystick-to-walk transitions.
    ///
    /// This state doesn't add any custom behavior beyond what PlayerItemState provides --
    /// it exists as a distinct type so the state machine can differentiate "at reception"
    /// from other item interactions.
    /// </summary>
    public sealed class PlayerReceptionState : PlayerItemState
    {
        public PlayerReceptionState(ItemController item) : base(item)
        {
            _item = item;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
