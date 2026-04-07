using Game.Core;
using Game.Level.Room;
using Injection;
using UnityEngine;

namespace Game.Level.Unit
{
    /// <summary>
    /// State for when a worker is inside a chamber excavating.
    /// Instead of running its own timer, listens to the room's ON_EXCAVATION_COMPLETE event.
    /// The room is the single source of truth for excavation timing.
    ///
    /// When excavation completes with multiple workers in the chamber, each worker leaves
    /// staggered by its slot index so their visuals don't overlap on exit.
    /// </summary>
    public sealed class UnitInRoomState : UnitState
    {
        private const float _unitSleepDistance = -1.5f;
        private const float _exitDelayMin = 1f; // each worker rolls a random exit delay in [min, max]
        private const float _exitDelayMax = 3f;

        [Inject] private LevelView _levelView;
        [Inject] private GameManager _gameManager;
        [Inject] private Timer _timer;

        private RoomController _room;
        private Vector3 _initLocalPosition;
        private float _exitDelay;
        private bool _exitPending;

        public override void Initialize()
        {
            if (!_gameManager.CustomerRoomMap.TryGetValue(_unit, out _room))
            {
                _unit.SwitchToState(new UnitIdleState());
                return;
            }

            // Get the slot anchor for this specific worker (set up in UnitWalkToRoomState.OnReachDistance
            // via room.AddWorker, which assigns an index in the active workers list).
            int slotIndex = _room.GetSlotIndex(_unit);
            var anchor = _room.View.GetCustomerPosition(slotIndex);

            _unit.View.transform.position = anchor.position;
            _unit.View.transform.eulerAngles = anchor.eulerAngles;

            _initLocalPosition = _unit.View.LocalTransform.localPosition;
            _unit.View.LocalTransform.localPosition = Vector3.forward * _unitSleepDistance;

            _unit.View.NavMeshAgent.enabled = false;
            _unit.View.Sleep();

            // Listen to the room — it tells us when excavation is done
            _room.ON_EXCAVATION_COMPLETE += OnExcavationComplete;
        }

        public override void Dispose()
        {
            _unit.View.LocalTransform.localPosition = _initLocalPosition;

            if (_exitPending)
            {
                _timer.TICK -= OnExitTick;
                _exitPending = false;
            }

            if (_room != null)
            {
                _room.ON_EXCAVATION_COMPLETE -= OnExcavationComplete;
                _room.RemoveWorker(_unit); // Free this worker's slot for the next dig cycle
            }
        }

        /// <summary>
        /// Fired the moment the chamber finishes its dig. Each worker rolls its own random
        /// exit delay so they walk out at different times instead of overlapping at the door.
        /// </summary>
        private void OnExcavationComplete()
        {
            _exitDelay = Random.Range(_exitDelayMin, _exitDelayMax);

            _exitPending = true;
            _timer.TICK += OnExitTick;
        }

        private void OnExitTick()
        {
            _exitDelay -= Time.deltaTime;
            if (_exitDelay > 0f) return;

            _timer.TICK -= OnExitTick;
            _exitPending = false;
            LeaveChamber();
        }

        private void LeaveChamber()
        {
            _gameManager.CustomerRoomMap.Remove(_unit);

            // Roll for a relic. Cash was already trickled during the dig (RoomOccupiedState),
            // so this is purely about whether the worker carries a physical artifact out to log
            // at the collector office. No relic → walk straight to the despawn point and skip
            // the office entirely.
            bool hasRelic = _gameManager.RollHasRelic();
            if (!hasRelic)
            {
                _unit.SwitchToState(new UnitWalkToRemoveState(_levelView.UnitRemovePoint.transform.position));
                return;
            }

            var toilet = _gameManager.FindToilet(_unit.Area);
            if (toilet != null)
            {
                var cabineResult = toilet.GetAvailableCabine();
                if (cabineResult != null)
                {
                    _unit.SwitchToState(new UnitWalkToCabineState(toilet, cabineResult));
                }
                else
                {
                    var place = toilet.Line.GetAvailablePlace();
                    if (place != null)
                    {
                        toilet.Line.PlaceUnitMap[place] = _unit;
                        _unit.SwitchToState(new UnitWalkState(place.transform.position));

                    } else _unit.SwitchToState(new UnitWalkToRemoveState(_levelView.UnitRemovePoint.transform.position));
                }

            }
            else _unit.SwitchToState(new UnitWalkToRemoveState(_levelView.UnitRemovePoint.transform.position));
        }
    }
}