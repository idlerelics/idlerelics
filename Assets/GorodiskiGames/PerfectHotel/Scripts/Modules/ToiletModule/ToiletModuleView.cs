using Game.Level.Toilet;
using UnityEngine;

namespace Game.Modules.ToiletModule
{
    /// <summary>
    /// View component for the ToiletModule. Automatically discovers all ToiletView
    /// components in its children on Awake, providing the module with references
    /// to every toilet facility in the current level.
    ///
    /// "sealed" means no other class can inherit from this one.
    /// MonoBehaviour is Unity's base class for components that attach to GameObjects.
    /// </summary>
    public sealed class ToiletModuleView : MonoBehaviour
    {
        /// <summary>
        /// Array of all toilet views found in child GameObjects.
        /// HideInInspector prevents this from showing in the Unity Inspector since
        /// it is populated automatically at runtime, not set by designers.
        /// </summary>
        [HideInInspector]
        public ToiletView[] ToiletViews;

        /// <summary>
        /// Awake is called by Unity when the GameObject is first created (before Start).
        /// GetComponentsInChildren searches this GameObject and all its children
        /// for ToiletView components and returns them as an array.
        /// This auto-discovery pattern means designers only need to place toilet
        /// prefabs as children -- no manual wiring required.
        /// </summary>
        private void Awake()
        {
            ToiletViews = GetComponentsInChildren<ToiletView>();
        }
    }
}
