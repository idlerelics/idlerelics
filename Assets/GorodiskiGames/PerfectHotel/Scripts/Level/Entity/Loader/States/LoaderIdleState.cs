using UnityEngine;

namespace Game.Level.Loader.LoaderStates
{
    /// <summary>
    /// The idle state for a loader -- the loader stands at its home position,
    /// facing forward (180 degrees on Y axis), with NavMesh navigation disabled.
    /// The idle animation plays based on the unit's sex and current inventory.
    ///
    /// Extends LoaderUpdateState, which handles the upgrade logic (checking if
    /// the loader can be upgraded and processing player payments). This means
    /// the loader can be upgraded while it's idling.
    /// </summary>
    public sealed class LoaderIdleState : LoaderUpdateState
    {
        /// <summary>
        /// Sets up the idle state: calls the base class to initialize upgrade logic,
        /// positions the loader facing forward, makes it visible, disables pathfinding,
        /// and plays the idle animation.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize(); // Set up upgrade monitoring from LoaderUpdateState

            // Face the loader toward the camera (180 degrees on Y axis)
            _loader.View.UnitView.transform.eulerAngles = new Vector3(0f, 180f, 0f);

            // Make the loader visible (it may have been hidden in a previous state)
            _loader.View.UnitView.Unhide();

            // Disable NavMeshAgent since the loader is standing still
            // (NavMeshAgent would override the position if left enabled)
            _loader.View.UnitView.NavMeshAgent.enabled = false;

            // Play the idle animation -- different animations based on sex and whether carrying items
            _loader.View.UnitView.Idle(_loader.UnitView.Sex, _loader.Inventories);
        }

        /// <summary>
        /// Cleans up by calling the base class Dispose (unsubscribes from upgrade events).
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
