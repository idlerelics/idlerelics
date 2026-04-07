using UnityEngine;

namespace Game.Level.Room
{
    /// <summary>
    /// State for when at least one worker is excavating in the chamber.
    /// Multi-worker behavior:
    ///  - Effective dig speed scales linearly with active workers (2 workers = 2× faster).
    ///  - Cash trickles every TRICKLE_INTERVAL_SECONDS, amount also scales with active workers
    ///    so the total payout stays equal to StayFee but is delivered faster with more workers.
    ///  - The state stays Occupied as long as at least one worker is inside, even if more arrive
    ///    or leave mid-dig. ON_WORKER_JOINED / ON_WORKER_LEFT trigger trickle recalculation.
    ///  - When the (worker-scaled) duration reaches 0, the room transitions to RoomUsedState.
    /// </summary>
    public sealed class RoomOccupiedState : RoomUpdateState
    {
        private float _stayDuration;        // Remaining base-time excavation work (worker-independent)
        private float _trickleTimer;        // Countdown to next cash trickle
        private int _baseTrickleAmount;     // Cash per trickle for a single worker
        private long _totalEarned;          // Cash already paid out
        private int _totalFee;              // Total StayFee target across the dig
        private int _numberOfTrickles;      // How many trickle ticks fit in the base duration

        private const float TRICKLE_INTERVAL_SECONDS = 2f;

        public override void Initialize()
        {
            base.Initialize();

            _room.AcceptingWorkers = true; // Late-arriving workers can still join the dig

            _stayDuration = _room.Model.StayDuration;
            _totalFee = _room.Model.StayFee;
            _totalEarned = 0;

            // Base trickle assumes 1 worker. With more workers, trickle amount is multiplied at payout time.
            // Cap trickle slots by total fee so per-trickle amount never integer-divides to 0
            // (would otherwise dump the entire fee as a single end-of-dig "remainder" payout
            // when fees are small — e.g. fee=5 / 7 slots = 0 per trickle).
            int slotsByTime = Mathf.Max(1, Mathf.FloorToInt(_stayDuration / TRICKLE_INTERVAL_SECONDS));
            _numberOfTrickles = Mathf.Max(1, Mathf.Min(slotsByTime, _totalFee));
            _baseTrickleAmount = Mathf.Max(1, _totalFee / _numberOfTrickles);
            _trickleTimer = TRICKLE_INTERVAL_SECONDS;

            // Light is driven by ActiveWorkerCount: dark while empty, lit as soon as the first
            // worker arrives, dark again when the last one leaves. The room can enter this state
            // before any worker has physically arrived, so honor the current count here.
            _room.View.SetDarkLight(_room.ActiveWorkerCount <= 0);

            _timer.TICK += OnTick;
            _room.ON_WORKER_JOINED += OnWorkersChanged;
            _room.ON_WORKER_LEFT += OnWorkersChanged;
        }

        public override void Dispose()
        {
            base.Dispose();

            _timer.TICK -= OnTick;
            _room.ON_WORKER_JOINED -= OnWorkersChanged;
            _room.ON_WORKER_LEFT -= OnWorkersChanged;
        }

        /// <summary>
        /// Each frame: counts down the dig and trickles cash. Both rates are multiplied
        /// by the number of active workers, giving linear parallel speedup.
        /// </summary>
        private void OnTick()
        {
            int workers = _room.ActiveWorkerCount;
            if (workers <= 0) return; // No one's here yet — wait for the first worker to arrive

            float dt = Time.deltaTime;
            float scaledDt = dt * workers;

            _stayDuration -= scaledDt;
            _trickleTimer -= dt;

            // Trickle cash periodically while still digging.
            // Amount scales with worker count so total payout matches StayFee.
            if (_trickleTimer <= 0f && _stayDuration > 0f)
            {
                int trickle = _baseTrickleAmount * workers;
                AddCash(trickle);
                _trickleTimer += TRICKLE_INTERVAL_SECONDS;
            }

            if (_stayDuration > 0f) return;

            // Excavation complete — pay out any remaining cash to avoid integer division loss
            long remainder = _totalFee - _totalEarned;
            if (remainder > 0)
            {
                AddCash(remainder);
            }

            _room.AcceptingWorkers = false;
            _room.ON_EXCAVATION_COMPLETE?.Invoke(); // Notify all units inside that the dig is done
            _room.SwitchToState(new RoomUsedState());
        }

        /// <summary>
        /// Called when a worker joins or leaves. The OnTick scaling handles speed automatically;
        /// this hook exists so we can extend later (visual feedback, audio cue, etc.).
        /// </summary>
        private void OnWorkersChanged()
        {
            // Toggle the chamber light: lit while at least one worker is inside, dark otherwise.
            // Scaling is still handled per-tick in OnTick.
            _room.View.SetDarkLight(_room.ActiveWorkerCount <= 0);
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
