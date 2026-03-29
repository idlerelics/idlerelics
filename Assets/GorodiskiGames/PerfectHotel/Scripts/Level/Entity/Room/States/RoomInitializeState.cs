using Injection;

namespace Game.Level.Room
{
    /// <summary>
    /// The first state a room enters. Determines the room's initial state based on
    /// saved progress data:
    /// - If purchased and area is purchased: check if it was left dirty (Used) or clean (Available)
    /// - If not purchased: check if the player has enough progress to show the purchase option
    ///   (ReadyToPurchase) or hide it completely (Hidden)
    ///
    /// This state is transient -- it always immediately transitions to another state.
    /// </summary>
    public sealed class RoomInitializeState : RoomState
    {
        [Inject] private GameManager _gameManager;

        public override void Initialize()
        {
            // Find which area this room belongs to
            var area = _gameManager.FindArea(_room.Model.Area);

            if (_room.Model.IsPurchased && area.Model.IsPurchased)
            {
                // Room and area are purchased -- restore saved state
                if (_gameManager.Model.LoadPlaceIsUsed(_room.Model.ID))
                    _room.SwitchToState(new RoomUsedState());      // Was dirty when game was saved
                else _room.SwitchToState(new RoomAvailableState()); // Was clean
            }
            else
            {
                // Room not purchased -- check if player can buy it
                if (_room.IsPurchasable(area.Model.IsPurchased, _gameManager.Model.LoadProgress(), _room.Model.TargetPurchaseValue))
                    _room.SwitchToState(new RoomReadyToPurchaseState()); // Show purchase option
                else _room.SwitchToState(new RoomHiddenState());         // Not enough progress, hide it
            }
        }

        public override void Dispose()
        {
        }
    }
}
