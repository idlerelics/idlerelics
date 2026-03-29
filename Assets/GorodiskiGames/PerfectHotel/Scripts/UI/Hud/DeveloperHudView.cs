using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    /// <summary>
    /// A debug/developer HUD panel that provides tools for testing the game.
    /// Allows developers to manually adjust the progress and level values using sliders,
    /// add cash, reset data, trigger rewarded ads, or reload the gameplay scene.
    ///
    /// This HUD is only intended for development and testing -- it should be
    /// hidden or disabled in production builds.
    ///
    /// "sealed" means no other class can inherit from DeveloperHudView.
    /// </summary>
    public sealed class DeveloperHudView : BaseHud
    {
        /// <summary>
        /// Action delegate invoked when the user releases a slider (pointer up).
        /// The mediator subscribes to this to save the updated values.
        ///
        /// "Action" is C#'s built-in delegate type for methods with no parameters
        /// and no return value.
        /// </summary>
        public Action SAVE;

        // Label prefixes for the slider display text
        private const string _progressWord = "PROGRESS ";
        private const string _lvlWord = "LEVEL ";

        // Developer tool buttons, assigned in the Inspector
        [SerializeField] private Button _closeButton;          // Close the dev HUD
        [SerializeField] private Button _addCashButton;        // Add a large amount of cash
        [SerializeField] private Button _loadGameplayButton;   // Reload the gameplay scene
        [SerializeField] private Button _resetButton;          // Reset all saved data
        [SerializeField] private Button _rewAdsButton;         // Simulate a rewarded ad

        // Progress slider UI elements
        [SerializeField] private TMP_Text _progressText;       // Shows "PROGRESS X"
        [SerializeField] private TMP_Text _progressMinText;    // Min label for progress slider
        [SerializeField] private TMP_Text _progressMaxText;    // Max label for progress slider
        [SerializeField] private Slider _progressSlider;       // Slider to adjust progress value

        // Level slider UI elements
        [SerializeField] private TMP_Text _lvlText;            // Shows "LEVEL X"
        [SerializeField] private TMP_Text _lvlMinText;         // Min label for level slider
        [SerializeField] private TMP_Text _lvlMaxText;         // Max label for level slider
        [SerializeField] private Slider _lvlSlider;            // Slider to adjust level value

        // Read-only properties for the buttons, so mediators can subscribe to click events
        public Button CloseButton => _closeButton;
        public Button AddCashButton => _addCashButton;
        public Button LoadGameplayButton => _loadGameplayButton;
        public Button ResetButton => _resetButton;
        public Button RewAdsButton => _rewAdsButton;

        // Cached values from the sliders, updated as the user drags
        int _progress;
        int _lvl;

        /// <summary>The current progress value selected by the slider.</summary>
        public int Progress => _progress;

        /// <summary>The current level value selected by the slider.</summary>
        public int Lvl => _lvl;

        /// <summary>
        /// Called when the user lifts their finger from a slider (pointer up event).
        /// Triggers the SAVE action so the mediator can persist the new values.
        /// The "?." (null-conditional operator) safely invokes only if SAVE has subscribers.
        /// </summary>
        public void OnPointerUp()
        {
            SAVE?.Invoke();
        }

        /// <summary>
        /// Called when the HUD becomes active. Subscribes to slider value changes
        /// so the display text updates in real-time as the user drags.
        ///
        /// Slider.onValueChanged is a Unity event that fires whenever the slider value changes.
        /// AddListener registers a callback method to be called when the event fires.
        /// </summary>
        protected override void OnEnable()
        {
            _progressSlider.onValueChanged.AddListener(OnProgressSlider);
            _lvlSlider.onValueChanged.AddListener(OnLvlSlider);
        }

        /// <summary>
        /// Called when the HUD becomes inactive. Unsubscribes from slider events
        /// to prevent callbacks firing on a disabled object.
        /// Always pair AddListener with RemoveListener to avoid memory leaks.
        /// </summary>
        protected override void OnDisable()
        {
            _progressSlider.onValueChanged.RemoveListener(OnProgressSlider);
            _lvlSlider.onValueChanged.RemoveListener(OnLvlSlider);
        }

        /// <summary>
        /// Sets the progress slider to a specific value and updates the display text.
        /// Called by the mediator when loading saved progress values.
        /// </summary>
        /// <param name="value">The progress value to set the slider to.</param>
        public void LoadProgress(int value)
        {
            _progressSlider.value = value;
            OnProgressSlider(value);
        }

        /// <summary>
        /// Sets the level slider to a specific value and updates the display text.
        /// Called by the mediator when loading saved level values.
        /// </summary>
        /// <param name="lvl">The level value to set the slider to.</param>
        public void LoadLvl(int lvl)
        {
            _lvlSlider.value = lvl;
            OnLvlSlider(lvl);
        }

        /// <summary>
        /// Configures the progress slider's minimum and maximum range.
        /// Also updates the min/max labels displayed alongside the slider.
        /// </summary>
        /// <param name="min">The minimum progress value.</param>
        /// <param name="max">The maximum progress value.</param>
        public void SetProgressSliderLimits(int min, int max)
        {
            _progressSlider.minValue = min;
            _progressSlider.maxValue = max;

            _progressMinText.text = min.ToString();
            _progressMaxText.text = max.ToString();
        }

        /// <summary>
        /// Configures the level slider's minimum and maximum range.
        /// Also updates the min/max labels displayed alongside the slider.
        /// </summary>
        /// <param name="min">The minimum level value.</param>
        /// <param name="max">The maximum level value.</param>
        public void SetLvlSliderLimits(int min, int max)
        {
            _lvlSlider.minValue = min;
            _lvlSlider.maxValue = max;

            _lvlMinText.text = min.ToString();
            _lvlMaxText.text = max.ToString();
        }

        /// <summary>
        /// Callback fired when the progress slider value changes.
        /// Casts the float value to int (sliders work with floats) and updates the label.
        /// </summary>
        /// <param name="value">The new slider value as a float.</param>
        private void OnProgressSlider(float value)
        {
            _progress = (int)value;
            _progressText.text = _progressWord + _progress;
        }

        /// <summary>
        /// Callback fired when the level slider value changes.
        /// Casts the float value to int and updates the label.
        /// </summary>
        /// <param name="value">The new slider value as a float.</param>
        private void OnLvlSlider(float value)
        {
            _lvl = (int)value;
            _lvlText.text = _lvlWord + _lvl;
        }
    }
}
