using Game.Level.Item;
using Game.Level.Toilet;
using UnityEngine;

namespace Game.Level.Unit
{
    /// <summary>
    /// State where a guest unit walks toward a toilet cabine (stall).
    /// While walking, the state monitors distance and opens the cabine door
    /// when the unit gets close enough, creating a natural-looking entrance.
    /// On arrival, the unit transitions to UnitInToiletCabineState.
    ///
    /// "sealed" means no other class can inherit from this one.
    /// Extends UnitWalkState, which handles NavMesh navigation and arrival detection.
    /// </summary>
    public sealed class UnitWalkToCabineState : UnitWalkState
    {
        /// <summary>Distance threshold (in world units) at which the cabine door opens.</summary>
        private const float _openDoorDistance = 2f;

        private ToiletController _toilet;       // The toilet facility this cabine belongs to
        private ItemToiletController _cabine;   // The specific cabine (stall) the unit is walking to
        private static Vector3 position;        // Placeholder required by the base constructor

        /// <summary>
        /// Constructor: stores the toilet and cabine references, and calculates
        /// the walk destination from the cabine's customer position (flattened to y=0
        /// so the unit walks on the ground plane).
        /// The "as" keyword casts the generic ItemController to the more specific
        /// ItemToiletController type, which has toilet-specific properties like door controls.
        /// </summary>
        public UnitWalkToCabineState(ToiletController toilet, ItemController item) : base(position)
        {
            _toilet = toilet;
            _cabine = item as ItemToiletController;

            // Use the cabine's customer position but force Y to 0 (ground level)
            _endPosition = new Vector3(_cabine.View.CustomerPosition.x, 0f, _cabine.View.CustomerPosition.z);
        }

        /// <summary>
        /// Starts NavMesh navigation and marks this cabine as occupied in the toilet's
        /// CabinesMap (false = occupied). Also subscribes to the timer to check
        /// door-opening distance each frame.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            // Mark the cabine as occupied (false) so no other unit tries to use it
            _toilet.CabinesMap[_cabine] = false;

            // Subscribe to frame tick to check when the unit is close enough to open the door
            _timer.TICK += OnTick;
        }

        /// <summary>
        /// Called when the unit reaches the cabine entrance.
        /// Transitions to UnitInToiletCabineState where the guest uses the toilet.
        /// </summary>
        public override void OnReachDistance()
        {
            _unit.SwitchToState(new UnitInToiletCabineState(_cabine));
        }

        /// <summary>
        /// Called every frame. When the unit gets within _openDoorDistance of the cabine,
        /// the door opens automatically. The tick listener is removed after opening
        /// to avoid re-triggering the door animation.
        /// </summary>
        private void OnTick()
        {
            // Wait until the unit is close enough to the cabine
            if ((_unit.View.transform.position - _endPosition).sqrMagnitude > _openDoorDistance * _openDoorDistance) return;

            // Open the cabine door as the unit approaches
            _cabine.View.OpenDoor();

            // Unsubscribe -- the door only needs to open once
            _timer.TICK -= OnTick;
        }

        /// <summary>
        /// Cleans up by unsubscribing from the timer tick event.
        /// This is important even though OnTick already unsubscribes itself,
        /// because the state might be disposed before the unit reaches the door
        /// (e.g., if the game reloads).
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            _timer.TICK -= OnTick;
        }
    }
}
