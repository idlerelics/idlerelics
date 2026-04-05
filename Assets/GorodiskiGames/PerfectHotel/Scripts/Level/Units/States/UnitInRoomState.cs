using Game.Level.Room;
using Injection;
using UnityEngine;

namespace Game.Level.Unit
{
    /// <summary>
    /// State for when a worker is inside a chamber excavating.
    /// Instead of running its own timer, listens to the room's ON_EXCAVATION_COMPLETE event.
    /// The room is the single source of truth for excavation timing.
    /// </summary>
    public sealed class UnitInRoomState : UnitState
    {
        private const float _unitSleepDistance = -1.5f;

        [Inject] private LevelView _levelView;
        [Inject] private GameManager _gameManager;

        private RoomController _room;
        private Vector3 _initLocalPosition;

        public override void Initialize()
        {
            if (!_gameManager.CustomerRoomMap.TryGetValue(_unit, out _room))
            {
                _unit.SwitchToState(new UnitIdleState());
                return;
            }

            _unit.View.transform.eulerAngles = _room.View.CustomerPosition.eulerAngles;

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

            if (_room != null)
                _room.ON_EXCAVATION_COMPLETE -= OnExcavationComplete;
        }

        private void OnExcavationComplete()
        {
            _gameManager.CustomerRoomMap.Remove(_unit);

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