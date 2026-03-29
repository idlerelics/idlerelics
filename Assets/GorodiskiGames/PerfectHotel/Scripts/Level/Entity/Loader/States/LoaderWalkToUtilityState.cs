using UnityEngine;

namespace Game.Level.Loader.LoaderStates
{
    /// <summary>
    /// State where the loader walks toward a utility station to deliver items.
    /// Extends LoaderWalkState which handles NavMesh pathfinding and distance checking.
    /// This is the counterpart to LoaderWalkToItemState -- the loader picks up at
    /// the item location and delivers here at the utility.
    ///
    /// LOADER WORKFLOW: Idle -> WalkToItem -> (pick up) -> WalkToUtility -> ArrivedToUtility -> Idle
    /// </summary>
    public sealed class LoaderWalkToUtilityState : LoaderWalkState
    {
        /// <summary>
        /// Constructs the state with the target utility position.
        /// </summary>
        /// <param name="position">World-space position of the utility station to deliver to.</param>
        public LoaderWalkToUtilityState(Vector3 position) : base(position)
        {
            _endPosition = position;
        }

        /// <summary>
        /// Calls the base Initialize to start NavMesh navigation toward the utility.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Calls the base Dispose to clean up walking behavior.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
        }

        /// <summary>
        /// Called by the base LoaderWalkState when the loader reaches the utility station.
        /// Triggers the ArrivedToUtility transition, which switches to a state that
        /// handles unloading items at the utility (e.g., restocking a soda machine).
        /// </summary>
        public override void OnReachDistance()
        {
            _loader.ArrivedToUtility();
        }
    }
}
