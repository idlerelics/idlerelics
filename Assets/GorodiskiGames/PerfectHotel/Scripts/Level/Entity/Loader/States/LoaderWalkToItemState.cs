using UnityEngine;

namespace Game.Level.Loader.LoaderStates
{
    /// <summary>
    /// State where the loader walks toward an item pickup location.
    /// Extends LoaderWalkState which handles the actual NavMesh pathfinding,
    /// walk animation, and distance checking. This subclass only needs to
    /// specify what happens when the loader reaches the destination.
    ///
    /// INHERITANCE CHAIN: LoaderWalkToItemState -> LoaderWalkState -> LoaderState -> State
    /// Each level adds behavior: State (lifecycle), LoaderState (shared deps),
    /// LoaderWalkState (NavMesh walking), LoaderWalkToItemState (arrival action).
    /// </summary>
    public sealed class LoaderWalkToItemState : LoaderWalkState
    {
        /// <summary>
        /// Constructs the state with the target position to walk toward.
        /// The position is passed to the base LoaderWalkState which handles pathfinding.
        /// </summary>
        /// <param name="position">World-space position of the item to pick up.</param>
        public LoaderWalkToItemState(Vector3 position) : base(position)
        {
            _endPosition = position;
        }

        /// <summary>
        /// Calls the base Initialize to start NavMesh navigation toward the target.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Calls the base Dispose to clean up walking behavior (stop NavMesh agent, etc.).
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
        }

        /// <summary>
        /// Called by the base LoaderWalkState when the loader is close enough
        /// to the target position. Fires the "arrived at item" event, which
        /// tells the loader system to pick up the item and transition to the
        /// next state (typically walking to a utility to deliver it).
        /// </summary>
        public override void OnReachDistance()
        {
            _loader.FireArrivedToItem();
        }
    }
}
