using Game.Level.Room;
using Injection;
using UnityEngine;

namespace Game.Level.Unit
{
    /// <summary>
    /// State where a guest unit walks from the reception line to their assigned room.
    /// On arrival, the unit enters UnitInRoomState and the room enters RoomOccupiedState.
    ///
    /// "sealed" means no other class can inherit from this one.
    /// Extends UnitWalkState, which handles NavMesh navigation and arrival detection.
    /// </summary>
    public sealed class UnitWalkToRoomState : UnitWalkState
    {
        // [Inject] tells the custom DI system to automatically fill this field
        // with the GameManager instance from the dependency injection context.
        [Inject] private GameManager _gameManager;

        private RoomController _room;      // The room this guest has been assigned to
        private static Vector3 position;   // Placeholder required by the base constructor

        /// <summary>
        /// Constructor: passes a default position to the base walk state.
        /// The actual destination is set in Initialize() once the room is known.
        /// </summary>
        public UnitWalkToRoomState() : base(position)
        {
        }

        /// <summary>
        /// Looks up the room assigned to this unit, sets the walk destination
        /// to the room's customer position, and removes the unit from the reception line.
        /// base.Initialize() starts the NavMesh navigation.
        /// </summary>
        public override void Initialize()
        {
            // Look up which room was assigned to this customer in the GameManager's mapping
            if (!_gameManager.CustomerRoomMap.TryGetValue(_unit, out _room))
            {
                _unit.SwitchToState(new UnitIdleState());
                return;
            }
            // Set the destination to the specific position inside the room where guests stand/sit
            _endPosition = _room.View.CustomerPosition.position;

            // Call base AFTER setting _endPosition so the NavMeshAgent walks to the correct spot
            base.Initialize();

            // Notify the reception line that this unit has left the queue
            _unit.FireUnitRemoveFromLine();
        }

        /// <summary>
        /// Called when the unit arrives at the room. Transitions both the unit
        /// and the room to their "occupied" states:
        /// - Unit enters UnitInRoomState (guest stays and eventually leaves)
        /// - Room enters RoomOccupiedState (generates income, blocks new guests)
        /// </summary>
        public override void OnReachDistance()
        {
            _unit.SwitchToState(new UnitInRoomState());
            _room.SwitchToState(new RoomOccupiedState());
        }

        /// <summary>Calls base to unsubscribe from timer events.</summary>
        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
