using UnityEngine;

namespace Game.Level.Room
{
    public sealed class RoomOccupiedState : RoomUpdateState
    {
        private float _stayDuration;

        public override void Initialize()
        {
            base.Initialize();

            _stayDuration = _room.Model.StayDuration;

            _room.View.SetDarkLight(true);

            _timer.TICK += OnTick;
        }

        public override void Dispose()
        {
            base.Dispose();

            _timer.TICK -= OnTick;
        }

        private void OnTick()
        {
            _stayDuration -= Time.deltaTime;

            if (_stayDuration > 0f) return;

            _room.Model.Cash += _room.Model.StayFee;
            _room.Model.SetChanged();
            _gameManager.Model.SavePlaceCash(_room.Model.ID, _room.Model.Cash);

            _room.SwitchToState(new RoomUsedState());
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


