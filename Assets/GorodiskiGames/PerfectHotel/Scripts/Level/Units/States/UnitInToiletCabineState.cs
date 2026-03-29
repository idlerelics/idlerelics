using Game.Level.Item;
using Injection;
using UnityEngine;

namespace Game.Level.Unit
{
    /// <summary>
    /// State where a guest unit is inside a toilet cabine (stall), using the facility.
    /// The unit plays a service animation for a set duration, then leaves:
    /// the door opens, the cabine fires an event to free itself up, and the unit
    /// walks to the level's removal point to be despawned.
    ///
    /// "sealed" means no other class can inherit from this one.
    /// </summary>
    public sealed class UnitInToiletCabineState : UnitState
    {
        // [Inject] tells the DI system to automatically provide the LevelView reference.
        // LevelView holds scene-wide references like spawn/remove points.
        [Inject] private LevelView _levelView;

        private ItemToiletController _cabine;   // The cabine this unit is occupying
        private float _toiletDuration;          // Countdown timer (seconds remaining)

        /// <summary>
        /// Constructor: stores a reference to the cabine the unit will occupy.
        /// </summary>
        public UnitInToiletCabineState(ItemToiletController cabine)
        {
            _cabine = cabine;
        }

        /// <summary>
        /// Enters the cabine: closes the door, disables NavMesh navigation (unit stays put),
        /// plays the appropriate service animation, rotates the unit to face the correct
        /// direction, and starts the countdown timer.
        /// </summary>
        public override void Initialize()
        {
            // Get how long this unit will stay in the cabine (configured per toilet level)
            _toiletDuration = _cabine.StayDuration;

            // Close the door behind the unit for privacy
            _cabine.View.CloseDoor();

            // Disable NavMeshAgent so the unit doesn't try to pathfind while inside
            _unit.View.NavMeshAgent.enabled = false;
            // Play the animation defined by this cabine type (e.g., sitting)
            _unit.View.Service(_cabine.View.UnitAnimationType);
            // Face the correct direction inside the cabine
            _unit.View.transform.eulerAngles = new Vector3(0f, _cabine.View.UnitAngle, 0f);

            // Subscribe to the global timer to count down each frame
            _timer.TICK += OnTick;
        }

        /// <summary>Unsubscribes from the timer to prevent callbacks after the state ends.</summary>
        public override void Dispose()
        {
            _timer.TICK -= OnTick;
        }

        /// <summary>
        /// Called every frame. Decrements the toilet duration by the time elapsed
        /// since the last frame (Time.deltaTime). When the timer reaches zero:
        /// 1. Opens the cabine door
        /// 2. Schedules the door to close again after a delay (for the next user)
        /// 3. Fires an event so the ToiletModule knows the cabine is free
        /// 4. Transitions the unit to walk toward the removal/despawn point
        /// </summary>
        private void OnTick()
        {
            // Count down using deltaTime (time between frames, usually ~0.016s at 60 FPS)
            _toiletDuration -= Time.deltaTime;

            // Keep waiting if time remains
            if (_toiletDuration > 0f) return;

            // Time's up -- open the door and schedule it to close after the unit leaves
            _cabine.View.OpenDoor();
            _cabine.View.CloseDoorWithDelay();

            // Notify the toilet system that this cabine is now available
            _cabine.FireUnitLeftToiletCabine();

            // Walk the unit to the removal point where it will be despawned/recycled
            _unit.SwitchToState(new UnitWalkToRemoveState(_levelView.UnitRemovePoint.transform.position));
        }
    }
}
