using UnityEngine;
using UnityEngine.AI;

namespace Game.Level.Cleaner
{
    /// <summary>
    /// Abstract base state for all cleaner walking behaviors.
    /// Sets up NavMesh navigation to a target position and polls each frame
    /// until the cleaner arrives within a small threshold (0.05 units).
    ///
    /// Subclasses (e.g., CleanerWalkToItemState, CleanerWalkHomeState) provide
    /// the destination and define what happens when the cleaner reaches it
    /// by implementing OnReachDistance().
    ///
    /// "abstract" means you cannot instantiate this class directly --
    /// you must create a subclass that implements OnReachDistance().
    /// </summary>
    public abstract class CleanerWalkState : CleanerUpdateState
    {
        /// <summary>The world-space position the cleaner is walking toward.</summary>
        public Vector3 _endPosition;

        /// <summary>
        /// Constructor: stores the target position.
        /// Subclasses may override _endPosition after calling base().
        /// </summary>
        public CleanerWalkState(Vector3 position)
        {
            _endPosition = position;
        }

        /// <summary>
        /// Activates the walk animation, enables NavMesh pathfinding, sets the
        /// destination and speed, then subscribes to the timer's TICK event
        /// to check arrival distance each frame.
        /// "base.Initialize()" calls CleanerUpdateState.Initialize() first,
        /// which sets up the upgrade/buy button logic.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            // Play the walk animation (0 = no items being carried)
            _cleaner.View.UnitView.Walk(0);

            // Enable the NavMeshAgent so it can calculate a path through the scene
            // NavMeshAgent is Unity's built-in AI navigation component
            _cleaner.View.UnitView.NavMeshAgent.enabled = true;
            _cleaner.View.UnitView.NavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            _cleaner.View.UnitView.NavMeshAgent.SetDestination(_endPosition);
            _cleaner.View.UnitView.NavMeshAgent.speed = _cleaner.Model.Speed;

            // Subscribe to the global timer to check distance every frame
            _timer.TICK += OnTick;
        }

        /// <summary>
        /// Called every frame via the Timer. Checks if the cleaner is close enough
        /// to the destination (within 0.05 world units). If not, returns early
        /// and lets the NavMeshAgent keep walking. Once close enough, triggers
        /// the arrival callback.
        /// </summary>
        private void OnTick()
        {
            // "return" exits early -- the cleaner hasn't arrived yet, keep walking
            if ((_cleaner.View.UnitView.transform.position - _endPosition).sqrMagnitude > 0.0025f) return;

            OnReachDistance();
        }

        /// <summary>
        /// Called when the cleaner arrives at the destination.
        /// Subclasses define what happens next (e.g., start cleaning, go idle).
        /// </summary>
        public abstract void OnReachDistance();

        /// <summary>
        /// Cleans up by unsubscribing from the timer tick event.
        /// Always unsubscribe from events in Dispose() to prevent memory leaks
        /// and "ghost" callbacks running after the state has ended.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            _timer.TICK -= OnTick;
        }
    }
}
