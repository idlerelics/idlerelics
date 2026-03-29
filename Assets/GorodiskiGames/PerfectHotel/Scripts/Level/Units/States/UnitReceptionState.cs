using UnityEngine;

namespace Game.Level.Unit
{
    /// <summary>
    /// State for a unit acting as a receptionist at the front desk.
    /// The receptionist stands in place, faces forward (180 degrees = toward the camera),
    /// and plays the reception animation. This is a static state with no transitions --
    /// the receptionist stays here until the module disposes of it.
    ///
    /// "sealed" means no other class can inherit from this one.
    /// </summary>
    public sealed class UnitReceptionState : UnitState
    {
        /// <summary>
        /// Disables NavMesh navigation (the receptionist doesn't move),
        /// plays the reception animation, and rotates the unit to face forward
        /// (180 degrees on the Y axis = facing toward the player/camera).
        /// </summary>
        public override void Initialize()
        {
            // Disable pathfinding -- the receptionist stays at the desk
            _unit.View.NavMeshAgent.enabled = false;
            // Play the reception greeting animation
            _unit.View.Reception();
            // Face the unit toward the camera (180 degrees around the Y axis)
            _unit.View.transform.eulerAngles = new Vector3(0f, 180f, 0f);
        }

        /// <summary>No cleanup needed -- the receptionist has no subscriptions or timers.</summary>
        public override void Dispose()
        {
        }
    }
}
