using UnityEngine;

namespace Game.UI.Hud
{
    /// <summary>
    /// A simple view component for a secondary camera setup used to render
    /// 3D content into a UI RawImage (render-to-texture technique).
    ///
    /// This is used by the Players HUD to display a 3D character preview
    /// in the UI. A separate camera renders the character to a RenderTexture,
    /// which is then displayed on a RawImage in the Canvas.
    ///
    /// "sealed" means no other class can inherit from RawCameraView.
    /// </summary>
    public sealed class RawCameraView : MonoBehaviour
    {
        // The Transform where the 3D character model should be placed for rendering
        [SerializeField] private Transform _spawnPlace;

        // The secondary camera that renders to a RenderTexture instead of the screen
        [SerializeField] private Camera _camera;

        /// <summary>The world-space position where objects should be spawned for this camera to render.</summary>
        public Transform SpawnPlace => _spawnPlace;

        /// <summary>The camera used for render-to-texture. Its targetTexture is set at runtime.</summary>
        public Camera Camera => _camera;
    }
}
