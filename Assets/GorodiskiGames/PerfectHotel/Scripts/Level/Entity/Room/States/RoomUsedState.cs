using Game.Level.Item;

namespace Game.Level.Room
{
    public sealed class RoomUsedState : RoomUpdateState
    {
        public override void Initialize()
        {
            base.Initialize();

            _gameManager.Model.SavePlaceIsUsed(_room.Model.ID, 1);

            _room.View.SetDarkLight(false);

            foreach (var item in _room.Items)
            {
                item.View.SetVisual(false, _room.Model.VisualIndex);

                item.Model.Duration = _room.Model.CleaningTime;
                item.Model.DurationNominal = item.Model.Duration;
                item.Model.SetChanged();

                _gameManager.AddItem(item);

                item.ITEM_FINISHED += OnItemFinished;
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            foreach (var item in _room.Items)
            {
                item.ITEM_FINISHED -= OnItemFinished;
            }
        }

        void OnItemFinished(ItemController item)
        {
            ItemRoomController itemRoom = item as ItemRoomController;

            item.ITEM_FINISHED -= OnItemFinished;

            itemRoom.View.SetVisual(true, _room.Model.VisualIndex);

            if (RoomCleaned())
                _room.SwitchToState(new RoomAvailableState());
        }

        private bool RoomCleaned()
        {
            foreach (var item in _room.Items)
            {
                if (item.Model.Duration > 0f)
                    return false;
            }
            return true;
        }

        public override void UpdateRoomVisual(int visualIndex)
        {
            base.UpdateRoomVisual(visualIndex);
            foreach (var item in _room.Items)
            {
                bool isAvailable = item.Model.Duration <= 0f;
                item.View.SetVisual(isAvailable, visualIndex);
            }
        }
    }
}


