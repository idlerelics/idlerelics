using Game.Level.Room;
using Injection;
using UnityEngine;

namespace Game.Level.Unit
{
    public sealed class UnitInRoomState : UnitState
    {
        private const float _unitSleepDistance = -1.5f;

        [Inject] private LevelView _levelView;
        [Inject] private GameManager _gameManager;

        private float _stayDuration;
        private RoomController _room;
        private Vector3 _initLocalPosition;

        public override void Initialize()
        {
            _room = _gameManager.CustomerRoomMap[_unit];
            _stayDuration = _room.Model.StayDuration;

            _unit.View.transform.eulerAngles = _room.View.CustomerPosition.eulerAngles;

            _initLocalPosition = _unit.View.LocalTransform.localPosition;
            _unit.View.LocalTransform.localPosition = Vector3.forward * _unitSleepDistance;

            _unit.View.NavMeshAgent.enabled = false;
            _unit.View.Sleep();

            _timer.TICK += OnTick;
        }

        public override void Dispose()
        {
            _unit.View.LocalTransform.localPosition = _initLocalPosition;

            _timer.TICK -= OnTick;
        }

        private void OnTick()
        {
            _stayDuration -= Time.deltaTime;

            if (_stayDuration > 0f) return;

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