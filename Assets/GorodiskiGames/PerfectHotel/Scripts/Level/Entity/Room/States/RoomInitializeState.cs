using Injection;

namespace Game.Level.Room
{
    public sealed class RoomInitializeState : RoomState
    {
        [Inject] private GameManager _gameManager;

        public override void Initialize()
        {
            var area = _gameManager.FindArea(_room.Model.Area);

            if (_room.Model.IsPurchased && area.Model.IsPurchased)
            {
                if (_gameManager.Model.LoadPlaceIsUsed(_room.Model.ID))
                    _room.SwitchToState(new RoomUsedState());
                else _room.SwitchToState(new RoomAvailableState());
            }
            else
            {
                if (_room.IsPurchasable(area.Model.IsPurchased, _gameManager.Model.LoadProgress(), _room.Model.TargetPurchaseValue))
                    _room.SwitchToState(new RoomReadyToPurchaseState());
                else _room.SwitchToState(new RoomHiddenState());
            }
        }

        public override void Dispose()
        {
        }
    }
}