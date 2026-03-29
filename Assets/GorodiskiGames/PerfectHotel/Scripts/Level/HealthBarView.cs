using Core;
using Game.UI.Hud;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Level.HealthBar
{
    /// <summary>
    /// Data model for a health bar. Inherits from Observable so the UI updates
    /// automatically when health values change (Observer pattern).
    /// </summary>
    public class HealthBarModel : Observable
    {
        public int Health;            // Current health value
        public int HealthNominal;     // Maximum health value (used to calculate fill percentage)
        public Color HealthBarColor;  // The color of the fill bar
    }

    /// <summary>
    /// UI view for displaying a health bar above entities (rooms, toilets, etc.).
    /// Uses the BehaviourWithModel pattern (BaseHudWithModel) to automatically
    /// respond to model changes.
    ///
    /// When the model's SetChanged() is called, OnModelChanged() fires automatically,
    /// updating the fill amount and text. The bar hides when health is full or zero.
    ///
    /// Image.fillAmount is a Unity UI property (0 to 1) that controls how much
    /// of the image is visible -- perfect for progress/health bars.
    /// </summary>
    public sealed class HealthBarView : BaseHudWithModel<HealthBarModel>
    {
        [SerializeField] private Image _fillImage;       // The bar fill image (uses fillAmount)
        [SerializeField] private TMP_Text _healthText;   // Text showing the numeric health value
        [SerializeField] private GameObject _holder;      // Parent object to show/hide the entire bar

        protected override void OnDisable()
        {
        }

        protected override void OnEnable()
        {
        }

        /// <summary>
        /// Called once when the model is first assigned. Sets the bar's color.
        /// </summary>
        protected override void OnApplyModel(HealthBarModel model)
        {
            base.OnApplyModel(model);

            _fillImage.color = model.HealthBarColor;
        }

        /// <summary>
        /// Called every time the model data changes (via SetChanged()).
        /// Updates the bar's visibility, fill amount, and numeric text.
        /// The bar is only visible when health is between 0 (exclusive) and max (exclusive).
        /// </summary>
        protected override void OnModelChanged(HealthBarModel model)
        {
            // Show the bar only when health is partially depleted (not full, not zero)
            _holder.SetActive(model.Health < model.HealthNominal && model.Health > 0);
            // Fill amount is a ratio: current / max (0.0 = empty, 1.0 = full)
            _fillImage.fillAmount = (float)model.Health / (float)model.HealthNominal;
            // Display the numeric health value
            _healthText.text = model.Health.ToString();
        }
    }
}
