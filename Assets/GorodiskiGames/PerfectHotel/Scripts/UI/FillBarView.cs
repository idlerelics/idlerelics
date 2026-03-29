using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    /// <summary>
    /// A reusable UI component that represents a fill/progress bar.
    /// Exposes references to its visual parts so that other scripts (controllers/mediators)
    /// can update the fill amount, show/hide the bar, or toggle a marker icon.
    ///
    /// This is a "view-only" component -- it holds no logic, just references to
    /// child UI elements that are assigned in the Unity Inspector via [SerializeField].
    ///
    /// "sealed" means no other class can inherit from FillBarView.
    /// </summary>
    public sealed class FillBarView : MonoBehaviour
    {
        // The parent container GameObject; toggle SetActive to show/hide the entire bar
        [SerializeField] private GameObject _holder;

        // An optional marker icon (e.g., a star or indicator) displayed on the bar
        [SerializeField] private GameObject _marker;

        // The Image component used for the fill. Set FillImage.fillAmount (0..1) to control progress.
        // The Image must have its Image Type set to "Filled" in the Inspector for fillAmount to work.
        [SerializeField] private Image _fillImage;

        /// <summary>The root container -- set active/inactive to show/hide the bar.</summary>
        public GameObject Holder => _holder;

        /// <summary>The fill Image -- change fillAmount (0 to 1) to update progress visually.</summary>
        public Image FillImage => _fillImage;

        /// <summary>An optional marker element displayed on or near the bar.</summary>
        public GameObject Marker => _marker;
    }
}
