using UnityEngine;

namespace Game.Level.Room
{
    /// <summary>
    /// State for when a guest is staying in the room.
    /// The room is occupied for a set duration (StayDuration).
    /// When the timer runs out, the guest's StayFee is added to the room's cash
    /// and the room transitions to RoomUsedState (dirty, needs cleaning).
    /// </summary>
    public sealed class RoomOccupiedState : RoomUpdateState
    {
        private float _stayDuration; // Countdown timer for how long the guest stays

        public override void Initialize()
        {
            base.Initialize();

            _stayDuration = _room.Model.StayDuration; // How long this guest stays

            _room.View.SetDarkLight(true); // Darken the room (guest is inside, do not disturb)

            _timer.TICK += OnTick; // Subscribe to per-frame updates for countdown
        }

        public override void Dispose()
        {
            base.Dispose();

            _timer.TICK -= OnTick;
        }

        /// <summary>
        /// Counts down the stay duration. When it reaches zero:
        /// 1. Adds the stay fee to the room's accumulated cash
        /// 2. Saves the updated cash to persistent storage
        /// 3. Transitions to RoomUsedState (dirty room)
        /// </summary>
        private void OnTick()
        {
            _stayDuration -= Time.deltaTime;

            if (_stayDuration > 0f) return; // Guest is still staying

            // Guest is leaving -- collect the stay fee
            _room.Model.Cash += _room.Model.StayFee;
            _room.Model.SetChanged(); // Notify UI observers
            _gameManager.Model.SavePlaceCash(_room.Model.ID, _room.Model.Cash);

            _room.SwitchToState(new RoomUsedState()); // Room is now dirty
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
