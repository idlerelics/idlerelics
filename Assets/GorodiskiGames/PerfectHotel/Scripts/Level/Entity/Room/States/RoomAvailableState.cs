namespace Game.Level.Room
{
    public sealed class RoomAvailableState : RoomUpdateState
    {
        public override void Initialize()
        {
            base.Initialize();

            _gameManager.Model.SavePlaceIsUsed(_room.Model.ID, 0);

            _room.IsAvailable = true;

            _room.View.SetDarkLight(false);

            foreach (var item in _room.View.Items)
            {
                item.SetVisual(true, _room.Model.VisualIndex);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void UpdateRoomVisual(int visualIndex)
        {
            base.UpdateRoomVisual(visualIndex);

            foreach (var item in _room.Items)
            {
                item.View.SetVisual(true, visualIndex);
            }
        }
    }
}

