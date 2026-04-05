using UnityEngine;

namespace Game.Level.Room
{
    /// <summary>
    /// State for when a worker is excavating in the chamber.
    /// Cash trickles out periodically during the excavation (not as a lump sum at the end).
    /// When the full duration expires, the room transitions to RoomUsedState.
    /// </summary>
    public sealed class RoomOccupiedState : RoomUpdateState
    {
        private float _stayDuration;        // Total excavation time remaining
        private float _trickleTimer;         // Countdown to next cash trickle
        private int _trickleAmount;          // Cash per trickle (integer)
        private long _totalEarned;           // Tracks what we've paid out so far
        private int _totalFee;               // Total StayFee to earn across the full stay
        private int _numberOfTrickles;       // How many trickle events to expect

        private const float TRICKLE_INTERVAL_SECONDS = 2f; // How often cash appears

        public override void Initialize()
        {
            base.Initialize();

            _stayDuration = _room.Model.StayDuration;
            _totalFee = _room.Model.StayFee;
            _totalEarned = 0;

            // Calculate trickle amounts: split StayFee evenly across the stay
            _numberOfTrickles = Mathf.Max(1, Mathf.FloorToInt(_stayDuration / TRICKLE_INTERVAL_SECONDS));
            _trickleAmount = _totalFee / _numberOfTrickles;
            _trickleTimer = TRICKLE_INTERVAL_SECONDS;

            _room.View.SetDarkLight(true);

            _timer.TICK += OnTick;
        }

        public override void Dispose()
        {
            base.Dispose();

            _timer.TICK -= OnTick;
        }

        /// <summary>
        /// Each frame: counts down both the stay duration and the trickle timer.
        /// Every trickle interval, a portion of cash appears at the chamber door.
        /// When the stay ends, any remaining unpaid cash is added and room transitions.
        /// </summary>
        private void OnTick()
        {
            float dt = Time.deltaTime;

            _stayDuration -= dt;
            _trickleTimer -= dt;

            // Trickle cash periodically while worker is still excavating
            if (_trickleTimer <= 0f && _stayDuration > 0f)
            {
                AddCash(_trickleAmount);
                _trickleTimer += TRICKLE_INTERVAL_SECONDS;
            }

            if (_stayDuration > 0f) return;

            // Excavation complete — pay out any remaining cash to avoid integer division loss
            long remainder = _totalFee - _totalEarned;
            if (remainder > 0)
            {
                AddCash(remainder);
            }

            _room.ON_EXCAVATION_COMPLETE?.Invoke(); // Notify unit(s) that excavation is done
            _room.SwitchToState(new RoomUsedState());
        }

        /// <summary>
        /// Adds cash to the room's pile and notifies observers (triggers visual cash spawn).
        /// </summary>
        private void AddCash(long amount)
        {
            _totalEarned += amount;
            _room.Model.Cash += amount;
            _room.Model.SetChanged();
            _gameManager.Model.SavePlaceCash(_room.Model.ID, _room.Model.Cash);
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
