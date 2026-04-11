using Game.Level.Room;
using Injection;
using UnityEngine;

namespace Game.Level.Unit
{
    /// <summary>
    /// State where a worker walks from the reception line to their assigned chamber.
    /// The chamber slot was already reserved by ReceptionModule via TryReserveSlot().
    ///
    /// On arrival:
    ///  - If the chamber is still accepting workers, the reservation is converted to an
    ///    active slot via room.AddWorker(unit), and the chamber transitions to (or stays in)
    ///    RoomOccupiedState. The walking destination uses the worker's specific slot anchor
    ///    so multiple workers don't stack on the same point.
    ///  - If the chamber is no longer accepting (e.g., dig finished while walking), the
    ///    reservation is released and the worker reroutes to the despawn point.
    /// </summary>
    public sealed class UnitWalkToRoomState : UnitWalkState
    {
        [Inject] private GameManager _gameManager;
        [Inject] private LevelView _levelView;

        private RoomController _room;
        private static Vector3 position;

        public UnitWalkToRoomState() : base(position)
        {
        }

        public override void Initialize()
        {
            if (!_gameManager.CustomerRoomMap.TryGetValue(_unit, out _room))
            {
                _unit.SwitchToState(new UnitIdleState());
                return;
            }

            // Walk toward the slot we'll occupy. Slot index = current active count
            // (the next slot the room will hand out via AddWorker).
            int targetSlot = _room.ActiveWorkerCount;
            _endPosition = _room.View.GetCustomerPosition(targetSlot).position;

            base.Initialize();

            _unit.FireUnitRemoveFromLine();
        }

        /// <summary>
        /// Called when the unit reaches the chamber. Tries to claim its reserved slot,
        /// and either joins the dig or reroutes to despawn if the chamber closed mid-walk.
        /// </summary>
        public override void OnReachDistance()
        {
            if (_room == null)
            {
                _unit.SwitchToState(new UnitWalkToRemoveState(_levelView.UnitRemovePoint.transform.position));
                return;
            }

            if (!_room.AcceptingWorkers)
            {
                // Chamber finished/closed while we were walking — release reservation, despawn
                _room.ReleaseReservation();
                _gameManager.CustomerRoomMap.Remove(_unit);
                _unit.SwitchToState(new UnitWalkToRemoveState(_levelView.UnitRemovePoint.transform.position));
                return;
            }

            // First worker triggers the room state transition; subsequent workers just join.
            if (_room.ActiveWorkerCount == 0)
                _room.SwitchToState(new RoomOccupiedState());

            _room.AddWorker(_unit);
            _unit.SwitchToState(new UnitInRoomState());
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
