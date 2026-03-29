using UnityEngine;

namespace Game.Level.Cash
{
    /// <summary>
    /// Visual representation of a cash/coin object in the game world.
    /// Handles showing/hiding the particle trail effect that follows the cash when it flies.
    ///
    /// [SerializeField] exposes private fields in the Unity Inspector so you can
    /// assign references by dragging GameObjects/Components in the Editor.
    /// This keeps the field private (good encapsulation) while still allowing
    /// Unity to serialize and display it.
    /// </summary>
    public class CashView : MonoBehaviour
    {
        [SerializeField] private GameObject _goTrail;       // The trail/particle effect GameObject
        [SerializeField] private Transform _rotationNode;   // The child transform used for spinning animations

        /// <summary>Public access to the rotation node for external animation control.</summary>
        public Transform Rotation => _rotationNode;

        /// <summary>
        /// Shows the trail effect by activating the GameObject.
        /// SetActive(true) makes a GameObject visible and enables its components.
        /// </summary>
        public void ShowTrail()
        {
            _goTrail.SetActive(true);
        }

        /// <summary>
        /// Hides the trail effect by deactivating the GameObject.
        /// SetActive(false) makes it invisible and disables all its components.
        /// </summary>
        public void HideTrail()
        {
            _goTrail.SetActive(false);
        }
    }
}
