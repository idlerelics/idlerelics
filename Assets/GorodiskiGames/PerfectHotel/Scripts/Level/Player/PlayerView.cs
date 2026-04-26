using System;
using Game.Level.Unit;
using UnityEngine;

namespace Game.Level.Player
{
    /// <summary>
    /// The visual representation of the player in the scene.
    /// "View" classes handle what you SEE -- meshes, positions, animations.
    /// This inherits from UnitStaffView, which likely extends MonoBehaviour
    /// (the base class for all Unity components attached to GameObjects).
    /// </summary>
    public sealed class PlayerView : UnitStaffView
    {
        // [SerializeField] makes a private field visible in the Unity Inspector,
        // so designers can drag-and-drop references without making the field public.
        [SerializeField] private Transform _aimTransform;          // Where the player is "aiming" or looking
        [SerializeField] private SkinnedMeshRenderer _body;        // The 3D mesh renderer for the player's body
        [SerializeField] private string _currentState;             // Debug display of current state name

        // "=>" is a shorthand (expression body) for a read-only property.
        public Vector3 AimPosition => _aimTransform.position;

        // Setting CurrentState stores the state's type name as a string for debugging.
        public Type CurrentState
        {
            set => _currentState = value?.Name;
        }

        /// <summary>
        /// Called when the player's data model changes.
        /// Updates the 3D body mesh to match the model's BodyMesh.
        /// "sharedMesh" is used instead of "mesh" to avoid creating a copy (saves memory).
        /// If the model provides a BodyMaterial, also swap it (per-character palette);
        /// otherwise the prefab's default material is kept.
        /// </summary>
        protected override void OnModelChanged(PlayerModel model)
        {
            // For per-character prefabs (Mixamo), the prefab already carries its own
            // mesh and material; PlayerConfig.Body is null and we leave the SMR untouched.
            if (model.BodyMesh != null)
                _body.sharedMesh = model.BodyMesh;
            if (model.BodyMaterial != null)
                _body.sharedMaterial = model.BodyMaterial;
        }
    }
}

