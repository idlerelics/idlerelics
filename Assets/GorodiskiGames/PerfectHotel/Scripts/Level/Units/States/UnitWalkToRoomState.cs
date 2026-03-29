using Game.Level.Room;
using Injection;
using UnityEngine;

namespace Game.Level.Unit
{
    public sealed class UnitWalkToRoomState : UnitWalkState
    {
        [Inject] private GameManager _gameManager;

        private RoomController _room;
        private static Vector3 position;

        public UnitWalkToRoomState() : base(position)
        {
        }

        public override void Initialize()
        {
            _room = _gameManager.CustomerRoomMap[_unit];
            _endPosition = _room.View.CustomerPosition.position;

            base.Initialize();

            _unit.FireUnitRemoveFromLine();
        }

        public override void OnReachDistance()
        {
            _unit.SwitchToState(new UnitInRoomState());
            _room.SwitchToState(new RoomOccupiedState());
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}